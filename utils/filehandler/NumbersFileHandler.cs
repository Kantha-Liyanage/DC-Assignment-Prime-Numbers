namespace dc.assignment.primenumbers.utils.filehandler
{
    public class NumbersFileHandler
    {
        private string[] fileLines;
        private string statusFile;
        private int currentNumberPosition = -1;
        public NumbersFileHandler(string inputFile, string statusFile)
        {
            this.fileLines = System.IO.File.ReadAllLines(inputFile);
            this.statusFile = statusFile;
            this.currentNumberPosition = int.Parse(System.IO.File.ReadAllText(statusFile));
        }

        //Previous number is completed automatically
        public int getNextNumber()
        {
            System.IO.File.WriteAllText(statusFile, this.currentNumberPosition.ToString());
            currentNumberPosition++;
            int currentNumber = int.Parse(fileLines[currentNumberPosition]);
            return currentNumber;
        }
    }
}