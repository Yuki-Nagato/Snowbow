using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowbow {
	public class TableOfContentsItem {
        public string Tag { get; }
        public string Text { get; }
        public string Id { get; }

        public TableOfContentsItem(string tag, string text, string id) {
            Tag = tag;
            Text = text;
            Id = id;
        }
    }
}
