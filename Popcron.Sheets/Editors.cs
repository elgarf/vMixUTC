using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class Editors
    {
        public string[] users;
        public string[] groups;
        public bool domainUsersCanEdit;
    }
}