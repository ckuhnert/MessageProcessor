using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    public class CSVMessage : EncodedMessage
    {
        public enum EOFStyle
        {
            LF,
            CR,
            CRLF
        }

        enum SearchingState
        {
            DetermineQuoteStyle,
            LookingForComma,
            LookingForCommaWithoutField,
            LookingForQuote,          
        }

        public CSVMessage(ISource source, Encoding encoding)
            : base(source, encoding)
        {
            LineStyle = EOFStyle.CRLF;
            Seperator = ',';
            Quote = '"';
            Escape = '\\';
        }

        public EOFStyle LineStyle { get; set; }
        public char Seperator { get; set; }
        public char Quote { get; set; }
        public char Escape { get; set; }

        private SearchingState state;
        private string currentField;
        public List<string> Fields;
        private bool AtEOL;
        private bool ContinueToNextLine;

        public bool GetLine()
        {
            char[] eol = null;
            Fields = new List<string>();

            switch (LineStyle)
            {
                case EOFStyle.LF:
                    eol = new char[] { '\r' };
                    break;
                case EOFStyle.CR:
                    eol = new char[] { '\n' };
                    break;
                case EOFStyle.CRLF:
                    eol = new char[] { '\r', '\n' };
                    break;
                default:
                    break;
            }

            state = SearchingState.DetermineQuoteStyle;

            string line = "";
            do
            {
                ContinueToNextLine = false;
                AtEOL = false;

                var lineBytes = GetUntil(eol);

                if (lineBytes == null)
                {
                    return false;
                }

                line = new string(lineBytes);
                line = line.TrimEnd(eol);

                int pos = 0;
                int fieldStartPos = 0;

                while (!AtEOL)
                {
                    switch (state)
                    {
                        case SearchingState.DetermineQuoteStyle:
                            ProcessDetermineQuote(line, ref pos, ref fieldStartPos);
                            break;
                        case SearchingState.LookingForCommaWithoutField:
                            ProcessLookForCommaWithoutField(line, ref pos, ref fieldStartPos);
                            break;
                        case SearchingState.LookingForComma:
                            ProcessLookForComma(line, ref pos, ref fieldStartPos);
                            break;
                        case SearchingState.LookingForQuote:
                            ProcessLookForQuote(line, ref pos, ref fieldStartPos);
                            break;
                        default:
                            break;
                    }
                }
            } while (ContinueToNextLine);

            return true;
        }

        private void ProcessLookForQuote(string line, ref int pos, ref int fieldStartPos)
        {
            int endPos = line.IndexOf(Quote, pos);
            while (endPos > 0 && line[endPos - 1] == Escape)
            {
                endPos = line.IndexOf(Quote, endPos);
            }

            if (endPos > 0)
            {
                // We have found a field
                state = SearchingState.LookingForCommaWithoutField;

                currentField += line.Substring(fieldStartPos, endPos - fieldStartPos);
                Fields.Add(currentField);
                pos = fieldStartPos = endPos + 1;
            }
            else
            {
                // we have hit the end of the line before we finished the quote
                AtEOL = true;
                ContinueToNextLine = true;

                currentField += line.Substring(fieldStartPos, line.Length - fieldStartPos);
                currentField += "\r\n";
            }
        }

        private void ProcessLookForCommaWithoutField(string line, ref int pos, ref int fieldStartPos)
        {
            int endPos = line.IndexOf(Seperator, pos);
            while (endPos > 0 && line[endPos - 1] == Escape)
            {
                endPos = line.IndexOf(Seperator, endPos);
            }

            if (endPos > 0)
            {
                // We have found a field
                state = SearchingState.DetermineQuoteStyle;

                pos = fieldStartPos = endPos + 1;
            }
            else
            {
                AtEOL = true;
            }
        }


        private void ProcessLookForComma(string line, ref int pos, ref int fieldStartPos)
        {
            int endPos = line.IndexOf(Seperator, pos);
            while (endPos > 0 && line[endPos - 1] == Escape)
            {
                endPos = line.IndexOf(Seperator, endPos);
            }

            if (endPos >= 0)
            {
                // We have found a field
                state = SearchingState.DetermineQuoteStyle;

                currentField += line.Substring(fieldStartPos, endPos - fieldStartPos);
                Fields.Add(currentField);
                pos = fieldStartPos = endPos + 1;
            }
            else
            {
                AtEOL = true;

                // We may have found the end of the line
                if  (pos < line.Length)
                {
                    currentField += line.Substring(fieldStartPos, line.Length - fieldStartPos);
                    Fields.Add(currentField);
                    pos = fieldStartPos = line.Length ;
                }
            }
        }

        private void ProcessDetermineQuote(string line, ref int pos, ref int fieldStartPos)
        {
            currentField = "";

            while (pos < line.Length && line[pos] == ' ')
            {
                currentField += " ";
                pos++;
            }

            if (pos >= line.Length)
            {
                AtEOL = true;

                Fields.Add(currentField);
                return;
            }

            if (line[pos] == Quote)
            {
                state = SearchingState.LookingForQuote;
                pos++;
                fieldStartPos++;
            }
            else
            {
                state = SearchingState.LookingForComma;
            }
        }
    }
}
