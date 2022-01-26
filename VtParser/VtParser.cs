using System;
using System.Text;

namespace VtParser
{
    public class VtParser
    {
        //private const int MAX_INTERMEDIATE_CHARS = 256;
        private States state;
        //private byte[] intermediate_chars = new byte[MAX_INTERMEDIATE_CHARS + 1];
        //private int num_intermediate_chars;
        private StringBuilder intermediate_chars = new StringBuilder();
        //private char ignore_flagged;
        private int[] params_ = new int[16];
        private int num_params;

        public delegate void CallbackFn(VtParser parser, Actions action, char ch);

        public CallbackFn Callback { get; set; }
        public string IntermediateChars => intermediate_chars.ToString();
        public Span<int> Parameters => params_.AsSpan(0, num_params);

        public VtParser()
        {
            state = States.VTPARSE_STATE_GROUND;
            intermediate_chars.Clear();
            num_params = 0;
            //ignore_flagged = (char)0;
        }

        public void PutChar(char c)
        {
            byte stateChange = Tables.StateTable[(int)state - 1, (byte) c];
            DoStateChange(stateChange, c);
        }

        public void PutString(string s)
        {
            foreach (char c in s)
            {
                PutChar(c);
            }
        }

        private void DoAction(Actions action, char ch)
        {
            /* Some actions we handle internally (like parsing parameters), others
             * we hand to our client for processing */

            switch (action)
            {
                case Actions.VTPARSE_ACTION_PRINT:
                case Actions.VTPARSE_ACTION_EXECUTE:
                case Actions.VTPARSE_ACTION_HOOK:
                case Actions.VTPARSE_ACTION_PUT:
                case Actions.VTPARSE_ACTION_OSC_START:
                case Actions.VTPARSE_ACTION_OSC_PUT:
                case Actions.VTPARSE_ACTION_OSC_END:
                case Actions.VTPARSE_ACTION_UNHOOK:
                case Actions.VTPARSE_ACTION_CSI_DISPATCH:
                case Actions.VTPARSE_ACTION_ESC_DISPATCH:
                    Callback?.Invoke(this, action, ch);
                    break;

                case Actions.VTPARSE_ACTION_IGNORE:
                    /* do nothing */
                    break;

                case Actions.VTPARSE_ACTION_COLLECT:
                    {
                        /* Append the character to the intermediate params */
                        //if (num_intermediate_chars + 1 > MAX_INTERMEDIATE_CHARS)
                        //    ignore_flagged = (char) 1;
                        //else
                        //    intermediate_chars[num_intermediate_chars++] = (byte) ch;
                        intermediate_chars.Append(ch);

                        break;
                    }

                case Actions.VTPARSE_ACTION_PARAM:
                    {
                        /* process the param character */
                        if (ch == ';')
                        {
                            num_params += 1;
                            params_[num_params-1] = 0;
                        }
                        else
                        {
                            /* the character is a digit */
                            int current_param;

                            if (num_params == 0)
                            {
                                num_params = 1;
                                params_[0]  = 0;
                            }

                            current_param = num_params - 1;
                            params_[current_param] *= 10;
                            params_[current_param] += (ch - '0');
                        }

                        break;
                    }

                case Actions.VTPARSE_ACTION_CLEAR:
                    intermediate_chars.Clear();
                    num_params = 0;
                    //ignore_flagged = (char) 0;
                    break;

                default:
                    Callback?.Invoke(this, Actions.VTPARSE_ACTION_ERROR, (char) 0);
                    break;
            }
        }

        private void DoStateChange(byte change, char ch)
        {
            /* A state change is an action and/or a new state to transition to. */

            States new_state = (States)(change >> 4);
            Actions action = (Actions)(change & 0x0F);


            if (new_state != States.UNDEFINED)
            {
                /* Perform up to three actions:
                 *   1. the exit action of the old state
                 *   2. the action associated with the transition
                 *   3. the entry action of the new state
                 */

                Actions exit_action = Tables.ExitActions[(int) state - 1];
                Actions entry_action = Tables.EntryActions[(int) new_state - 1];

                if (exit_action != Actions.UNDEFINED)
                    DoAction(exit_action, (char) 0);

                if (action != Actions.UNDEFINED)
                    DoAction(action, ch);

                if (entry_action != Actions.UNDEFINED)
                    DoAction(entry_action, (char)0);

                state = new_state;
            }
            else
            {
                DoAction(action, ch);
            }
        }
    }
}
