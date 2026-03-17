// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("HJ+Rnq4cn5ScHJ+fngBqH9p/oI+31Ts61Omw1STCxme5ayy+jH0a/64cn7yuk5iXtBjWGGmTn5+fm56dRK1HtatUPXk+AbecjOL2bMskDJFactxNneIjU/JeiOojfHqnc5+QNyz83h/M1BgjGITfNvGUknODiy5uBffxEt3je2bzaYpq/2mEP5IEIo2eltH/1JffkHqUKmaSffn5mabTkMZJBI9zDlNLDal+uFyss5nj6fESwOkizKQe1oQlCWizho6OBq3sZpTyo6KI2CzZK3WU1icBflCYo4vYMRHQHX6zeTNXrW4vaI+ivi7L4n4+Tr3Z/jmi9m//VhWwfUJ0qOhOCjz7u+m2l+kko4BuwSj+D6LmoI0ndzEqdH1U4E7CaZydn56f");
        private static int[] order = new int[] { 1,5,5,10,6,6,9,11,11,12,13,13,12,13,14 };
        private static int key = 158;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
