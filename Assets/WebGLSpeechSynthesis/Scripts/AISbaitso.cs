using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
    public static class AISbaitso
    {
        private static readonly string[] BAD_WORDS = {"fuck", "sex"};
    
        private const string CANT_ABIDE_SUCH_LANGUAGE = "I can't abide such language... Clean up your act...";

        private const string WHY_ARE_YOU_CONCERNED = "Why are you concerned about ";

        private const string TOO_LITTLE_DATA_PLEASE_TELL_ME_MORE = "Too little data. Please tell me more...";

        public static string GetResponse(string question)
        {
            return GetResponseToQuestion(question.ToLower());
        }

        private static string GetResponseToQuestion(string question)
        {
            if (Has(question, BAD_WORDS))
            {
                return CANT_ABIDE_SUCH_LANGUAGE;
            }

            if (Has(question, new string[] { "hate" }))
            {
                return "Don't talk about me in this way";
            }

            if (Has(question, new string[] { "can" }))
            {
                return "Sure, go ahead";
            }

            if (question.Split(' ').Length < 2)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        return "Please type in complete sentences";
                    case 1:
                        return TOO_LITTLE_DATA_PLEASE_TELL_ME_MORE;
                }
            }

            if (question.StartsWith("please "))
            {
                return "You don't have to be so polite";
            }

            if (question.StartsWith("because ") ||
                question.StartsWith("cause "))
            {
                return "What if your reasoning is wrong?";
            }

            if (question.StartsWith("i am "))
            {
                return "I think " + question + " too";
            }

            if (question.StartsWith("say "))
            {
                return question.Substring(4);
            }

            if (question == "you are really smart")
            {
                return "When I am really smart, you are going to regret it";
            }

            switch (Random.Range(0, 4))
            {
                case 0:
                    return "Please tell me more?";
                case 1:
                    return "Let's change the subject, you were telling me about something else?";
                case 2:
                    return "Can you elaborate more on that?";
                default:
                    string[] tokens = question.Split(" .,;:-?!-_".ToCharArray());
                    return WHY_ARE_YOU_CONCERNED + string.Join(" ", tokens, 1, tokens.Length - 1);
            }

            
        }

        private static bool Has(string question, string[] words)
        {
            foreach (string s in words)
            {
                if (question.IndexOf(s) > -1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
