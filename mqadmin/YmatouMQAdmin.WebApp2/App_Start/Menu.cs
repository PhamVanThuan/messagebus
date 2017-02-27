using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace YmatouMQAdmin.WebApp2.App_Start
{

    public class MenuNode
    {
        public MenuNode()
        {
            this.ChildNodes = new List<MenuNode>();
        }

        public string Text { get; set; }

        public string Title { get; set; }

        public string Target { get; set; }

        public string NavigateUrl { get; set; }

        public int Tabindex { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public IList<MenuNode> ChildNodes { get; set; }

        public override string ToString()
        {
            MenuLiNode liNode = new MenuLiNode();
            if (string.IsNullOrWhiteSpace(this.Class) && this.ChildNodes != null && this.ChildNodes.Count > 0)
                this.Class = "dropdown-submenu";
            liNode.Class = this.Class;
            MenuANode aNodel = new MenuANode();
            aNodel.Text = this.Text;
            aNodel.Title = this.Title;
            aNodel.Target = this.Target;
            aNodel.NavigateUrl = this.NavigateUrl;
            aNodel.Tabindex = this.Tabindex;
            aNodel.HasDropdown = this.Level == 0 && this.ChildNodes != null && this.ChildNodes.Count > 0;
            if (this.Level <= 0)
                aNodel.HasCaret = true;
            liNode.Content += aNodel.ToString();
            if (this.ChildNodes != null && this.ChildNodes.Count > 0)
            {
                MenuUlNode ulNode = new MenuUlNode();
                ulNode.Class = "dropdown-menu";
                for (int i = 0; i < this.ChildNodes.Count; i++)
                {
                    MenuNode menuNode = this.ChildNodes[i];
                    if (menuNode == null)
                        continue;
                    menuNode.Tabindex = i;
                    menuNode.Level = this.Level + 1;
                    ulNode.Content += menuNode.ToString();
                }
                liNode.Content += ulNode.ToString();
            }
            return liNode.ToString();
        }

        public static string ShowMenu(IList<MenuNode> nodes, int number = 10, string ulClass = null)
        {

            if (nodes == null || nodes.Count <= 0)
                return string.Empty;

            if (nodes.Count > number)
            {
                IList<MenuNode> newNodes = new List<MenuNode>();
                for (int i = 0; i < number; i++)
                {
                    newNodes.Add(nodes[i]);
                }
                MenuNode moreNode = new MenuNode();
                moreNode.Text = "更多...";
                for (int j = number; j < nodes.Count; j++)
                {
                    moreNode.ChildNodes.Add(nodes[j]);
                }
                newNodes.Add(moreNode);
                nodes = newNodes;
            }

            MenuUlNode ulNode = new MenuUlNode();
            if (string.IsNullOrWhiteSpace(ulClass))
                ulClass = "nav navbar-nav";
            ulNode.Class = ulClass;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                    continue;
                nodes[i].Tabindex = i;
                if (string.IsNullOrWhiteSpace(nodes[i].Class))
                    nodes[i].Class = "dropdown";
                ulNode.Content += nodes[i].ToString();
            }
            return ulNode.ToString();
        }
    }


    public class MenuLiNode
    {
        public string Class { get; set; }

        public string Content { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<li class=\"{0}\">", this.Class);
            sb.Append(this.Content);
            sb.Append("</li>");
            return sb.ToString();
        }
    }

    public class MenuANode
    {
        public int Tabindex { get; set; }


        public string Text { get; set; }

        public string Title { get; set; }

        public string Target { get; set; }

        public string NavigateUrl { get; set; }


        public bool HasDropdown { get; set; }

        public bool HasCaret { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<a ");
            sb.AppendFormat(" tabindex=\"{0}\" ", this.Tabindex);
            if (this.HasDropdown)
            {
                sb.Append(" data-toggle=\"dropdown\" data-submenu ");
            }
            if (!string.IsNullOrWhiteSpace(this.Title))
                sb.AppendFormat(" title=\"{0}\" ", this.Title);

            if (!string.IsNullOrWhiteSpace(this.NavigateUrl))
            {
                sb.AppendFormat(" href=\"{0}\" ", this.NavigateUrl);

                if (!string.IsNullOrWhiteSpace(this.Target))
                {
                    sb.AppendFormat(" target=\"{0}\" ", this.Target);
                }
            }

            sb.Append(">");

            sb.Append(this.Text);
            if (this.HasCaret)
            {
                sb.Append("<span class=\"caret\"></span>");
            }
            sb.Append("</a>");
            return sb.ToString();
        }

    }

    public class MenuUlNode
    {
        public string Class { get; set; }

        public string Content { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<ul class=\"{0}\" >", this.Class);
            sb.Append(this.Content);
            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}