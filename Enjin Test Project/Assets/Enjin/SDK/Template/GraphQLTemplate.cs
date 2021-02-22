using System.Collections.Generic;
using UnityEngine;

namespace Enjin.SDK.Template
{
    public class GraphQLTemplate
    {
        public Dictionary<string, string> GetQuery { get; private set; }

        public GraphQLTemplate(string templateFile)
        {
            GetQuery = new Dictionary<string, string>();
            ReadTemplate(templateFile);
        }

        private void ReadTemplate(string file)
        {
            TextAsset templateData = Resources.Load<TextAsset>("Templates/" + file);

            if (templateData.text == string.Empty)
                return;

            string[] lines = templateData.text.Split('\n');

            foreach (string line in lines)
            {
                if (line != null || line != "")
                {
                    string[] cmd = line.Split('|');
                    GetQuery.Add(cmd[0], cmd[1]);
                }
            }
        }
    }
}