using System;

namespace dc.assignment.primenumbers.utils.file
{
    public class NumbersFileHandler
    {
        private string inputFile;
        private string outputFile;
        private string[] fileLines;
        private int currentNumberPosition = -1;
        public NumbersFileHandler(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
        }

        public void readAllNumber()
        {
            this.fileLines = System.IO.File.ReadAllLines(inputFile);
            this.currentNumberPosition = -1;
        }

        public int getNextNumber()
        {
            if (this.fileLines == null)
            {
                return -1;
            }

            // next number
            currentNumberPosition++;
            int currentNumber = int.Parse(fileLines[currentNumberPosition]);

            // check whether already checked
            while (true)
            {
                string[] completedFileLines = System.IO.File.ReadAllLines(outputFile);
                bool alreadyChecked = (Array.IndexOf(completedFileLines, currentNumber) > -1);
                if (!alreadyChecked)
                {
                    break;
                }
                currentNumber = int.Parse(fileLines[currentNumberPosition]);
            }
            return currentNumber;
        }

        public bool writeResult(int theNumber, bool isPrime)
        {
            System.IO.File.AppendAllText(this.outputFile, theNumber.ToString() + ":" + isPrime.ToString());
            return true;
        }
    }
}