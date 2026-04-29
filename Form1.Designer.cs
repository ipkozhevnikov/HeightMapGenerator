using Silk.NET.Maths;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Reflection;

namespace orthoplane
{

    /*
    4.5 система реконструкции поверхности движения по размещенным по 
    периметру робота 4 стереокамерамм:
    на роботизированной платформе расположено 4 стереокамеры,
    ориентированные по сторонам света (в разные стороны) со склонением 45 градусов. 
    Стереокамеры возвращают карты глубины.
    На основании данных карт глубин, положения и ориентации робота
    построить карту высот (в формате ортоплана - вид сверху) 
    окружения робота размером 2х2 м в формате 1000х1000 пикселей.
    */

    partial class MainForm
    {

        private Bitmap[] originalDepthMaps = new Bitmap[4];
        private bool hasDephMap = false;
        private Bitmap resultHeightMap;
     
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1015, 637);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private void InitializeCustomComponents()
        {
            this.Text = "Depth to Height Map Converter";
            this.Size = new Size(1200, 800);

            // Кнопки загрузки
            string[] titles = ["спереди", "справа", "сзади", "слева"];
            for (int i = 0; i < 4; i++)
            {
                var btnLoad = new Button
                {
                    Text = "Карта глубины " + titles[i],
                    Location = new Point(20, 20 + 45 * i),
                    Size = new Size(150, 40)
                };
                btnLoad.Click += BtnLoad_Click;
                btnLoad.Tag = i;
                this.Controls.Add(btnLoad);
            }

            // Кнопка преобразования
            var btnConvert = new Button
            {
                Text = "Сгенерировать карту высот",
                Location = new Point(180, 20),
                Size = new Size(150, 40),
                Enabled = false
            };
            btnConvert.Click += BtnConvert_Click;
            this.Controls.Add(btnConvert);

            // Кнопка сохранения
            var btnSave = new Button
            {
                Text = "Сохранить результат",
                Location = new Point(340, 20),
                Size = new Size(150, 40),
                Enabled = false
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Параметры камеры
            AddLabel("Высота камеры по оси Z:", 180, 80);
            var numHeight = AddNumericUpDown(1.0m, 0.1m, 10.0m, 450, 80);

            AddLabel("Угол отклонения камеры от оси Z:", 180, 120);
            var numTilt = AddNumericUpDown(-135m, -180m, 180m, 450, 120);
            numTilt.Increment = 5;

            AddLabel("Фокусное расстояние камеры (мм):", 180, 160);
            var numFocal = AddNumericUpDown(7m, 1.0m, 10.0m, 450, 160);

            var picOriginals = new PictureBox[4];
            Size size = new Size(150, 150);
            // PictureBox для исходного изображения
            picOriginals[0] = new PictureBox
            {
                Location = new Point(170, 220),
                Size = size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(picOriginals[0]);
            picOriginals[1] = new PictureBox
            {
                Location = new Point(320, 370),
                Size = size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(picOriginals[1]);
            picOriginals[2] = new PictureBox
            {
                Location = new Point(170, 520),
                Size = size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(picOriginals[2]);
            picOriginals[3] = new PictureBox
            {
                Location = new Point(20, 370),
                Size = size,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(picOriginals[3]);

            // PictureBox для результата
            var picResult = new PictureBox
            {
                Location = new Point(540, 220),
                Size = new Size(500, 500),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(picResult);

            // Сохраняем ссылки на элементы управления
            //this.Controls.Add(btnLoad);
            this.Controls.Add(btnConvert);
            this.Controls.Add(btnSave);
            this.Tag = new { btnConvert, btnSave, picOriginals, picResult, numHeight, numTilt, numFocal };
        }

        private void NumTilt_Click(object sender, EventArgs e)
        {
            RunHeightMap();
        }

        private void AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
            this.Controls.Add(label);
        }

        private NumericUpDown AddNumericUpDown(decimal value, decimal min, decimal max, int x, int y)
        {
            var num = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = 2,
                Increment = 0.1m,
                Location = new Point(x, y),
                Width = 80
            };
            this.Controls.Add(num);
            return num;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int index = (int)(sender as Button).Tag;
                    originalDepthMaps[index] = new Bitmap(openFileDialog.FileName);
                    var controls = (dynamic)this.Tag;
                    controls.picOriginals[index].Image = originalDepthMaps[index];
                    controls.btnConvert.Enabled = true;
                    controls.btnSave.Enabled = false;
                    hasDephMap = true;
                }
            }
        }

        /**
         * экспорт координат точек в файл
         */
        public static void ExportToXYZ(List<Vector3d> points, string filePath)
        {
            var lines = new List<string>();

            foreach (var point in points)
            {
                // Форматируем каждую точку: X Y Z
                lines.Add($"{point.X} {point.Y} {point.Z}");
            }

            File.WriteAllLines(filePath, lines);
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            RunHeightMap();
        }

        private void RunHeightMap()
        {
            if (!hasDephMap)
                return;

            int outputWidth = 1000;
            int outputHeight = 1000;
            List<Vector3d> heightMap = new List<Vector3d>();
            var controls = (dynamic)this.Tag;

            // Получаем параметры из элементов управления
            float cameraHeight = (float)controls.numHeight.Value;//высота расположения камеры
            float cameraTilt = (float)controls.numTilt.Value;//угол наклона камеры
            float focalLength = (float)controls.numFocal.Value; //фокусное расстояние, мм
            float sensorWidth = 6.0f; //ширина сенсора камеры, мм

            for (int i=0; i<4; i++)
            {
                if (originalDepthMaps[i] == null)
                {
                    continue;
                }
                float pixelDensity = originalDepthMaps[i].Width / sensorWidth;//плотность пикселей на мм, px/мм
                float focalLengthPx = focalLength * pixelDensity;//фокусное расстояние, px
                var camera = new Camera(new Vector3d(0, cameraHeight, 0), i * -90, 0, cameraTilt, focalLengthPx);
                // Преобразование карты глубин в облако точек
                List<Vector3d> voxelMap = DepthTransformer.ConvertDepthToVoxelMap(
                    originalDepthMaps[i],
                    camera);
                heightMap.AddRange(voxelMap);
            }
            //Camera camera = new Camera(new Vector3d(0, cameraHeight, 0), 90, 0, cameraTilt, focalLengthPx);

            //ExportToXYZ(voxelMap, "d:\\xyz.xyz");
            //Bitmap heightMapBitmap = HeightMapGenerator.CreateHeightMapWithInterpolation(heightMap, 1000, 1000);
            //heightMapBitmap.Save("d:\\heightmap.png", System.Drawing.Imaging.ImageFormat.Png);

            //крайние значения координат
            double left = float.MaxValue, right = float.MinValue, top = float.MinValue, bottom = float.MaxValue, min = float.MaxValue, max = float.MinValue;
            foreach (var point in heightMap)
            {
                if (point.X < left)
                    left = point.X;
                if (point.X > right)
                    right = point.X;
                if (point.Y < bottom)
                    bottom = point.Y;
                if (point.Y > top)
                    top = point.Y;
                if (point.Z < min)
                    min = point.Z;
                if (point.Z > max)
                    max = point.Z;
            }
            double sizeX = right - left;
            double sizeY = top - bottom;
            double scaleX = sizeX / outputWidth;
            double scaleY = sizeY / outputHeight;
            double scale = Math.Max(scaleX, scaleY) + 0.05;
            max += 5;//немного отступить от крайних значений белого и черного для лучшей картинки   
            min -= 5;
            foreach (var point in heightMap)
            {
                point.Z = (point.Z - min) / (max - min) * 255; // размазываем значения по шкале 0..255
                //point.Z = point.Z - min;
            }

            resultHeightMap = new Bitmap(outputWidth, outputHeight);
            using (var g = Graphics.FromImage(resultHeightMap))
                g.Clear(Color.Black);
            foreach (var point in heightMap)
            {
                point.X = (point.X - left - sizeX / 2) / scale + outputWidth / 2;// помещаем в центр изображения
                point.Y = (point.Y - bottom - sizeY / 2) / scale + outputHeight / 2;// помещаем в центр изображения
                point.Y = outputHeight - point.Y;//т.к. ось y смотрит вниз
                int zColor = (int)point.Z;
                //закрашиваем если не вылезли за границы
                //if (zColor > 0 && zColor <= 255 && point.X >= 0 && point.X < outputWidth && point.Y >= 0 && point.Y < outputHeight)
                {
                    Color color = ElevationColorMap.GetElevationColorAsColor(zColor);
                    //закрашиваем пиксель если он ярче (выше) предыдущего
                    if (resultHeightMap.GetPixel((int)point.X, (int)point.Y).R < color.R)
                    {
                        resultHeightMap.SetPixel((int)point.X, (int)point.Y, color);
                    }
                }
            }
            // Отображаем результат
            controls.picResult.Image = resultHeightMap;
            controls.btnSave.Enabled = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (resultHeightMap == null) return;

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG изображение|*.png";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    resultHeightMap.Save(saveFileDialog.FileName, ImageFormat.Png);
                    MessageBox.Show("Карта высот сохранена", "Успешное сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
