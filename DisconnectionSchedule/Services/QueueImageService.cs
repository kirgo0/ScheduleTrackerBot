using DisconnectionSchedule.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DisconnectionSchedule.Services
{
    public class QueueImageService : IQueueImageService
    {
        private readonly string _imagesPath;
        private readonly Font _font;

        public QueueImageService()
        {
            _imagesPath = "images";

            var collection = new FontCollection();
            var family = collection.AddSystemFonts().Families.First(); // or load any TTF
            _font = family.CreateFont(16, FontStyle.Regular);
        }

        public async Task<string> GenerateImageAsync(QueueData data, string outputPath)
        {
            const int columnWidth = 30;
            const int headerHeight = 50; // provides space for rotated hours
            const int rowHeight = 42;

            int totalColumns = 24;
            int width = columnWidth * totalColumns;
            int height = headerHeight + rowHeight;

            using var image = new Image<Rgba32>(width, height, Color.White);

            // Load status icons
            var icons = new Dictionary<PowerStatus, Image>
            {
                { PowerStatus.Yes, Image.Load(Path.Combine(_imagesPath, "yes.png")) },
                { PowerStatus.No, Image.Load(Path.Combine(_imagesPath, "no.png")) },
                { PowerStatus.First, Image.Load(Path.Combine(_imagesPath, "first.png")) },
                { PowerStatus.Second, Image.Load(Path.Combine(_imagesPath, "second.png")) },
            };

            var black = Color.Black;

            image.Mutate(ctx =>
            {
                // Draw header hours rotated 90°
                for (int i = 0; i < totalColumns; i++)
                {
                    string text = $"{i:00}-{i + 1:00}";

                    var textImg = new Image<Rgba32>(headerHeight, columnWidth);
                    textImg.Mutate(t =>
                    {
                        t.Fill(Color.Transparent);
                        t.DrawText(text, _font, Color.Black, new PointF(5, 5));
                        t.Rotate(90); // Vertical
                    });

                    ctx.DrawImage(textImg, new Point(i * columnWidth, 0), 1f);
                }

                // Draw status icons
                for (int i = 0; i < totalColumns; i++)
                {
                    data.HourlyStatus.TryGetValue(i, out var status);

                    if (!icons.ContainsKey(status))
                        continue;

                    var icon = icons[status];

                    int x = i * columnWidth + (columnWidth - icon.Width) / 2;
                    int y = headerHeight + (rowHeight - icon.Height) / 2;

                    ctx.DrawImage(icon, new Point(x, y), 1f);
                }

                // Draw grid
                for (int i = 0; i <= totalColumns; i++)
                {
                    int x = i * columnWidth;
                    ctx.DrawLine(black, 1, new PointF(x, 0), new PointF(x, height));
                }

                ctx.DrawLine(black, 2, new PointF(0, headerHeight), new PointF(width, headerHeight));
                ctx.DrawLine(black, 2, new PointF(0, height), new PointF(width, height));

            });

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await image.SaveAsync(outputPath);

            return outputPath;
        }
    }
}
