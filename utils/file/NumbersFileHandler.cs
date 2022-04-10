using System;

namespace dc.assignment.primenumbers.utils.file
{
    public class NumbersFileHandler
    {
        private string inputFile;
        private string completedFile;
        private string outputFile;
        private string[] fileLines;
        private int currentNumberPosition = -1;
        public NumbersFileHandler(string inputFile, string completedFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.completedFile = completedFile;
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

            if (currentNumberPosition == fileLines.Length)
            {
                return -1;
            }

            // get next number
            int currentNumber = int.Parse(fileLines[currentNumberPosition]);

            // check whether already checked
            while (true)
            {
                Console.WriteLine("check whether already checked");
                string[] completedFileLines = System.IO.File.ReadAllLines(completedFile);
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
            System.IO.File.AppendAllText(this.completedFile, theNumber.ToString());
            System.IO.File.AppendAllText(this.outputFile, theNumber.ToString() + ":" + isPrime.ToString());
            return true;
        }
    }
}