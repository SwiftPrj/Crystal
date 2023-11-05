namespace Crystal
{
    class Crystal
    {
        public static void Main(string[] args)
        {
            string path = @"../../../tests/test_general.cy";
            Parser parser = new Parser(path);
            parser.parse();
        }
    }
}