using System.Drawing;

namespace CliFx.Models
{
    public class TextSpan
    {
        public string Text { get; }

        public Color Color { get; }

        public TextSpan(string text, Color color)
        {
            Text = text;
            Color = color;
        }

        public TextSpan(string text)
            : this(text, Color.Gray)
        {
        }

        public override string ToString() => Text;
    }
}