namespace Level.Platforms
{
    public enum BiomePart
    {
        None,
        Left,
        Middle,
        Right,
        CrustLeft,
        CrustMiddle,
        CrustRight,
        UnderLeft,
        UnderMiddle,
        UnderRight,
        Wall,
        CornerLeft,
        CornerRight,
        Fill,
        EndSide
    }

    public static class BiomeFileNames
    {
        private const string Prefix = "Env_";
        private const char Separator = '_';

        public static BiomePart Parse(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith(Prefix))
                return BiomePart.None;

            string[] tokens = fileName.Split(Separator);
            int last = tokens.Length - 1;

            if (last < 2)
                return BiomePart.None;

            if (IsNumber(tokens[last]))
                last--;

            string part = tokens[last];
            string group = last > 0 ? tokens[last - 1] : string.Empty;

            switch (part)
            {
                case "Wall":
                    return BiomePart.Wall;
                case "Fill":
                    return BiomePart.Fill;
                case "EndSide":
                    return BiomePart.EndSide;
                case "Left":
                    return Side(group, BiomePart.Left, BiomePart.CrustLeft, BiomePart.UnderLeft, BiomePart.CornerLeft);
                case "Right":
                    return Side(group, BiomePart.Right, BiomePart.CrustRight, BiomePart.UnderRight, BiomePart.CornerRight);
                case "Middle":
                    return Side(group, BiomePart.Middle, BiomePart.CrustMiddle, BiomePart.UnderMiddle, BiomePart.None);
                default:
                    return BiomePart.None;
            }
        }

        private static BiomePart Side(string group, BiomePart plain, BiomePart top, BiomePart bottom, BiomePart corner)
        {
            switch (group)
            {
                case "Top":
                    return top;
                case "Bottom":
                    return bottom;
                case "CornerOut":
                    return corner;
                default:
                    return plain;
            }
        }

        private static bool IsNumber(string token)
        {
            if (token.Length == 0)
                return false;

            for (int i = 0; i < token.Length; i++)
            {
                if (!char.IsDigit(token[i]))
                    return false;
            }

            return true;
        }
    }
}
