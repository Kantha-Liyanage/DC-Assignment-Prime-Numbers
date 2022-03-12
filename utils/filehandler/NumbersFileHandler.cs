namespace dc.assignment.primenumbers.utils.filehandler
{
    public class NumbersFileHandler
    {
        private string dataFile;
        private string statusFile;
        private string[] fileLines;
        private int currentNumberPosition = -1;
        public NumbersFileHandler(string dataFile, string statusFile)
        {
            this.dataFile = dataFile;
            this.statusFile = statusFile;
        }

        public void loadData()
        {
            this.fileLines = System.IO.File.ReadAllLines(dataFile);
            this.currentNumberPosition = int.Parse(System.IO.File.ReadAllText(statusFile));
        }

        //Previous number is completed automatically
        public int getNextNumber()
        {
            if (this.fileLines == null)
            {
                return -1;
            }

            System.IO.File.WriteAllText(statusFile, this.currentNumberPosition.ToString());
            currentNumberPosition++;
            int currentNumber = int.Parse(fileLines[currentNumberPosition]);
            return currentNumber;
        }
    }
}