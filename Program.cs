namespace Crystal
{
    class Crystal
    {
        public static void Main(string[] args)
        {
            string path = @"C:\Users\twisted\source\repos\Crystal\bin\Release\net7.0\script.cy";
            Parser parser = new Parser(path);
            parser.parse();
        }
    }
}