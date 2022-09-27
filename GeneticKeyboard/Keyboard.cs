using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticKeyboard
{
    internal enum KeyboardGenType
    {
        None,
        Default
    }

    internal class Keyboard
    {
        public static char[] AllKeys = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
            'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', ',', '.', ';', '\'' };

        public static char[] ResetKeys = new char[] { ' ', '\n' };

        public int[] KeyboardLayout = new int[AllKeys.Length];
        // 0 -> 9 top row
        // 10 -> 20 second row
        // 21 -> 29 bottom row

        public float Fitness = -1;

        private int[] fingerPositions = new int[8];

        public Keyboard(bool qwerty = false)
        {
            if (!qwerty)
            {
                Random random = new Random();
                int[] key_path = new int[AllKeys.Length];

                for (int i = 0; i < AllKeys.Length; i++)
                {
                    key_path[i] = random.Next(AllKeys.Length - i);
                }

                List<int> keys_remaining = new List<int>();

                for (int i = 0; i < AllKeys.Length; i++)
                {
                    keys_remaining.Add(i);
                }

                for (int i = 0; i < AllKeys.Length; i++)
                {
                    KeyboardLayout[i] = keys_remaining[key_path[i]];
                    keys_remaining.RemoveAt(key_path[i]);
                }
            }
            else
            {
                string s = "qwertyuiopasdfghjkl;'zxcvbnm,.";

                for (int i = 0; i < KeyboardLayout.Length; i++)
                {
                    int index = -1;
                    for (int j = 0; j < AllKeys.Length; j++)
                    {
                        if (AllKeys[j] == s[i])
                        {
                            index = j;
                            break;
                        }
                    }

                    KeyboardLayout[i] = index;
                }
            }
        }

        public Keyboard(Keyboard parent1, Keyboard parent2, KeyboardGenType genType = KeyboardGenType.Default)
        {
            switch (genType)
            {
                case KeyboardGenType.None:
                    KeyboardLayout = parent1.KeyboardLayout;
                    break;

                case KeyboardGenType.Default:
                    Random r = new Random();

                    for (int i = 0; i < KeyboardLayout.Length; i++)
                    {
                        KeyboardLayout[i] = -1;
                    }

                    List<int> unused_keys = new List<int>();
                    for (int i = 0; i < AllKeys.Length; i++)
                    {
                        unused_keys.Add(i);
                    }

                    for (int i = 0; i < KeyboardLayout.Length; i++)
                    {
                        int primary;
                        int secondary;
                        if (r.Next(2) == 0) 
                        {
                            primary = parent1.KeyboardLayout[i];
                            secondary = parent2.KeyboardLayout[i];
                        }
                        else
                        {
                            primary = parent2.KeyboardLayout[i];
                            secondary = parent1.KeyboardLayout[i];
                        }

                        if (!KeyboardLayout.Contains(primary))
                        {
                            KeyboardLayout[i] = primary;
                            unused_keys.Remove(primary);
                        }
                        else if (!KeyboardLayout.Contains(primary)) 
                        { 
                            KeyboardLayout[i] = secondary; 
                            unused_keys.Remove(secondary); 
                        }
                    }

                    for (int i = 0; i < KeyboardLayout.Length; i++)
                    {
                        if (KeyboardLayout[i] == -1)
                        {
                            int rand = r.Next(unused_keys.Count());
                            KeyboardLayout[i] = unused_keys[rand];
                            unused_keys.RemoveAt(rand);
                        }
                    }

                    break;
            }
        }

        public void CalculateFitness()
        {
            Fitness = FitnessEvaluator.GetFitness(this);
        }

        public void ResetFingers() { fingerPositions = new int[8]; }

        public float GetTravelTime(char c)
        {
            int char_index = -1;

            for (int i = 0; i < AllKeys.Length; i++)
            {
                if (AllKeys[i] == c) char_index = i;
            }


            int key_pos = -1;

            for (int i = 0; i < KeyboardLayout.Length; i++)
            {
                if (KeyboardLayout[i] == char_index) key_pos = i;
            }

            int row = 0;
            int col = 0;

            if (key_pos < 10)
            {
                row = 0;
                col = key_pos;
            }
            else if (key_pos < 21)
            {
                row = 1;
                col = key_pos - 10;
            }
            else
            {
                row = 2;
                col = key_pos - 21;
            }

            if (col < 4 || col == 7 || col == 8)
            {
                int finger = col;
                if (col > 4)
                {
                    finger -= 2;
                }

                float travel_time = Math.Abs(fingerPositions[finger] - (-row + 1));
                fingerPositions[finger] = (-row + 1);

                float modifier;
                switch (finger)
                {
                    case 0:
                        modifier = 1.35f;
                        break;
                    case 1:
                        modifier = 1.25f;
                        break;
                    case 2:
                        modifier = 1.15f;
                        break;
                    case 5:
                        modifier = 1.1f;
                        break;
                    default:
                        modifier = 1.2f;
                        break;

                }

                return travel_time * modifier;
            }
            else if (col == 4 || col == 5)
            {
                Tuple<int, int> finger_pos = FingerPosToCoords(fingerPositions[3]);

                int new_finger_pos;

                switch (row, col)
                {
                    case (0, 4):
                        new_finger_pos = 2;
                        break;
                    case (0, 5):
                        new_finger_pos = 3;
                        break;
                    case (1, 4):
                        new_finger_pos = 0;
                        break;
                    case (1, 5):
                        new_finger_pos = 1;
                        break;
                    case (2, 4):
                        new_finger_pos = -1;
                        break;
                    default:
                        new_finger_pos = -2;
                        break;
                }

                Tuple<int, int> new_finger_coords = FingerPosToCoords(new_finger_pos);

                float travel_time = DistBetweenCoords(finger_pos, new_finger_coords);

                fingerPositions[3] = new_finger_pos;

                return travel_time * 1.05f;
            }
            else if (col == 6 || col == 7)
            {
                Tuple<int, int> finger_pos = FingerPosToCoords(fingerPositions[4]);

                int new_finger_pos;

                switch (row, col)
                {
                    case (0, 7):
                        new_finger_pos = 2;
                        break;
                    case (0, 6):
                        new_finger_pos = 3;
                        break;
                    case (1, 7):
                        new_finger_pos = 0;
                        break;
                    case (1, 6):
                        new_finger_pos = 1;
                        break;
                    case (2, 7):
                        new_finger_pos = -1;
                        break;
                    default:
                        new_finger_pos = -2;
                        break;
                }

                Tuple<int, int> new_finger_coords = FingerPosToCoords(new_finger_pos);

                float travel_time = DistBetweenCoords(finger_pos, new_finger_coords);

                fingerPositions[4] = new_finger_pos;

                return travel_time;
            }
            else
            {
                Tuple<int, int> finger_pos = FingerPosToCoords(fingerPositions[5]);

                int new_finger_pos;

                switch (row, col)
                {
                    case (0, 9):
                        new_finger_pos = 2;
                        break;
                    case (1, 9):
                        new_finger_pos = 0;
                        break;
                    case (1, 10):
                        new_finger_pos = 1;
                        break;
                    default:
                        new_finger_pos = -1;
                        break;
                }

                Tuple<int, int> new_finger_coords = FingerPosToCoords(new_finger_pos);

                float travel_time = DistBetweenCoords(finger_pos, new_finger_coords);

                fingerPositions[5] = new_finger_pos;

                return travel_time * 1.3f;
            }
        }

        private float DistBetweenCoords(Tuple<int, int> pos1, Tuple<int, int> pos2)
        {
            return MathF.Sqrt(MathF.Pow(pos1.Item1 - pos2.Item1, 2) + MathF.Pow(pos1.Item2 - pos2.Item2, 2));
        }

        private Tuple<int, int> FingerPosToCoords(int pos)
        {
            switch (pos)
            {
                case 1:
                    return Tuple.Create(1, 0);
                case 2:
                    return Tuple.Create(0, 1);
                case 3:
                    return Tuple.Create(1, 1);
                case -1:
                    return Tuple.Create(0, -1);
                case -2:
                    return Tuple.Create(1, -1);
                default:
                    return Tuple.Create(0, 0);
            }
        }
    }
}
