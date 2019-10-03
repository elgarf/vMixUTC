using System;

namespace vMixController.Classes
{
    public class SMPTEReader
    {
        private const int SYNC = 0xBFFC;
        private bool _bigEndian = false;
        private int _channels;
        private int _frameSize;
        // Timecode decoder variables (holds state between called to process() method)
        private bool _skipBit;
        private bool _bitValue;
        private bool _dropDetect;
        private int[] _frame = new int[4];
        private int _bitCount = 0;
        private int _frameIndex = 0;
        private int _lastInterval = 0;
        private int _frameWord = 0;
        private int _lastSample;
        private int _interval;
        private int _lastFrame;
        private int _lastSecond;
        private int _frameCount;
        private string _timecode;

        public int FrameCount { get => _frameCount; set => _frameCount = value; }
        public bool DropDetect { get => _dropDetect; set => _dropDetect = value; }
        public string Timecode { get => _timecode; set => _timecode = value; }

        public SMPTEReader(int channels, int frameSize)
        {
            _channels = channels;
            _frameSize = frameSize;
        }

        public void ProcessFrame(byte[] buffer, int count)
        {
            int[] samples = new int[count / 2];
            if (_channels == 1 && _frameSize == 2)
            {
                if (_bigEndian)
                {
                    // Handle big Endian data
                    for (int ii = 0; ii < count; ii += 2)
                    {
                        samples[ii >> 1] = (buffer[ii] << 8) | (buffer[ii + 1] & 0xFF);
                    }
                }
                else
                {
                    // Handle little Endian data
                    for (int ii = 0; ii < count; ii += 2)
                    {
                        samples[ii >> 1] = (buffer[ii + 1] << 8) | (buffer[ii] & 0xFF);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Audio format not recognized!");
            }
            // Compute Input Signal Level
            double level = 0;
            foreach (var sample in samples)
            {
                level += Math.Abs(sample);
            }
            level = (level / (double)samples.Length) / (double)0x4000;
            //levelMeter.setValue((int)(100 * level));
            // Count bit intervals by watching zero crossing
            foreach (var sample in samples)
            {
                int dx = _lastSample - sample;
                bool lastSign = _lastSample > 0;
                bool sampleSign = sample > 0;
                if (Math.Abs(dx) > 10 && lastSign != sampleSign)
                {
                    if (_skipBit)
                    {
                        // skip 2nd half of '1' bit
                        _skipBit = false;
                    }
                    else
                    {
                        if (_interval > (_lastInterval + (_lastInterval >> 1)))
                        {
                            // transitioned to a '0' bit
                            _frameWord = _frameWord >> 1;
                            _bitValue = false;
                        }
                        else if (_interval < (_lastInterval - (_lastInterval >> 2)))
                        {
                            // transitioned to a '1' bit
                            _frameWord = (_frameWord >> 1) + 0x8000;
                            _bitValue = true;
                            _skipBit = true;
                        }
                        else
                        {
                            // same as last bit
                            _frameWord >>= 1;
                            if (_bitValue)
                            {
                                _frameWord |= 0x8000;
                                _skipBit = true;
                            }
                        }
                        // Look for end of frame sync pattern 0b1011111111111100;
                        if (_frameWord == SYNC)
                        {
                            if (_frameIndex == 4)
                            {
                                // Time code frame received in frame[]
                                decodeFrame(_frame);
                            }
                            _frameIndex = 0;
                            _bitCount = 0;
                            _frameWord = 0;
                        }
                        else if (++_bitCount >= 16)
                        {
                            if (_frameIndex < 4)
                            {
                                _frame[_frameIndex++] = _frameWord;
                            }
                            else
                            {
                                _frameIndex = 0;
                            }
                            _bitCount = 0;
                        }
                    }
                    _lastInterval = _interval;
                    _interval = 0;
                }
                else
                {
                    _interval++;
                }
                _lastSample = sample;
            }
        }

        /*
         *  buf[0]  | u  u  u  u | C  d  F  F | u  u  u  u | f  f  f  f |  Frames
         *           15 14 13 12  11 10 9  8    7  6  5  4   3  2  1  0
         *
         *  buf[1]  | u  u  u  u | x  S  S  S | u  u  u  u | s  s  s  s |  Seconds
         *           31 30 29 28  27 26 25 24  23 22 21 20  19 18 17 16
         *
         *  buf[2]  | u  u  u  u | x  M  M  M | u  u  u  u | m  m  m  m |  Minutes
         *           47 46 45 44  43 42 41 40  39 38 37 36  35 34 33 32
         *
         *  buf[3]  | u  u  u  u | x  c  H  H | u  u  u  u | h  h  h  h |  Hours
         *           63 62 61 60  59 58 57 56  55 54 53 52  51 50 49 49
         *
         *  buf[4]  | 1  0  1  1 | 1  1  1  1 | 1  1  1  1 | 1  1  0  0 |  SYNC Pattern
         *           79 78 77 76  75 74 73 72  71 70 69 68  67 66 65 64
         *
         *  u = User bits, Upper case = tens, lower case = units
         *  d = drop frame flag, c = clock sync flag, C = color frame flag
         *  x = special flag bits bits (usage depends on frame rate)
         *  if 25 fps
         *    bit 59 is polarity correction bit
         *    bit 27 is BFG0 and bit 43 is BFG2
         *  else
         *    bit 27 is polarity correction bit (obsolete, or reassigned)
         *    bit 43 is BFG0 and bit 59 is BFG2
         *  if BFG0 == 1 user bits contain four 8 bit chars, else unspecified data
         */

        private void decodeFrame(int[] frame)
        {
            int frmUnits = frame[0] & 0x0F;
            int frmTens = (frame[0] >> 8) & 0x03;
            int secUnits = frame[1] & 0x0F;
            int secTens = (frame[1] >> 8) & 0x07;
            int minUnits = frame[2] & 0x0F;
            int minTens = (frame[2] >> 8) & 0x07;
            int hrsUnits = frame[3] & 0x0F;
            int hrsTens = (frame[3] >> 8) & 0x03;
            // Error check on data
            if (frmUnits > 9 || secUnits > 9 || minUnits > 9 || hrsUnits > 9)
            {
                return;
            }
            // Get flags
            bool bit10 = (frame[0] & 0x400) != 0;       // Bit 10 drop frame (if 30 fps)
            bool bit11 = (frame[0] & 0x800) != 0;       // Bit 11 Color frame flag (if 30, or 25 fps)
            bool bit27 = (frame[1] & 0x800) != 0;       // Bit 27
            bool bit43 = (frame[2] & 0x800) != 0;       // Bit 43
            bool bit58 = (frame[3] & 0x400) != 0;       // Bit 58 also called the BFG1 flag
            bool bit59 = (frame[3] & 0x800) != 0;       // Bit 59 phase-correction bit
            /*user59.setSelected(bit59);
            user58.setSelected(bit58);
            user43.setSelected(bit43);
            user27.setSelected(bit27);
            user11.setSelected(bit11);
            user10.setSelected(bit10);*/
            // Unpack User Bit Fields
            /*String userFields =
                Integer.toHexString((frame[3] >> 12) & 0x0F) +
                    Integer.toHexString((frame[3] >> 4) & 0x0F) +
                    Integer.toHexString((frame[2] >> 12) & 0x0F) +
                    Integer.toHexString((frame[2] >> 4) & 0x0F) +
                    Integer.toHexString((frame[1] >> 12) & 0x0F) +
                    Integer.toHexString((frame[1] >> 4) & 0x0F) +
                    Integer.toHexString((frame[0] >> 12) & 0x0F) +
                    Integer.toHexString((frame[0] >> 4) & 0x0F);
            userData.setText(userFields);*/
            // Check for Dropped Frames
            int frameNum = frmTens * 10 + frmUnits;
            int minsNum = minTens * 10 + minUnits;
            if (frameNum == 2 && _lastFrame != 1)
            {
                _dropDetect = minsNum % 10 != 0;
            }
            _lastFrame = frameNum;
            // Update Frame Rate Calculation
            if (secUnits != _lastSecond)
            {
                _lastSecond = secUnits;
                //frameRate.setText((_frameCount + 1) + " fps" + (_dropDetect ? " (drop frame)" : ""));
                _frameCount = 0;
            }

            _frameCount = Math.Max(_frameCount, frmUnits + frmTens * 10);
            // Format timecode as ASCII String
            _timecode = string.Format("{0}{1}:{2}{3}:{4}{5}{6}{7}{8}", hrsTens, hrsUnits, minTens, minUnits, secTens, secUnits, bit10 ? ";" : ":", frmTens, frmUnits);
            /*Integer.toString(hrsTens) +
                Integer.toString(hrsUnits) +
                ":" +
                Integer.toString(minTens) +
                Integer.toString(minUnits) +
                ":" +
                Integer.toString(secTens) +
                Integer.toString(secUnits) +
                (bit10 ? ";" : ":") +
                Integer.toString(frmTens) +
                Integer.toString(frmUnits);*/
            //timeView.setText(buf);
            /*switch (recordWhat)
            {
                case "Timecode":
                    timecodeLog.append(buf + "\n");
                    break;
                case "TC + Raw Frame":
                    timecodeLog.append(buf + " - ");
                case "Raw Frame":
                    String hex = toHex(frame[3]) + ':' + toHex(frame[2]) + ':' + toHex(frame[1]) + ':' + toHex(frame[0]) + '\n';
                    timecodeLog.append(hex);
                    break;
            }*/
        }
    }

}
