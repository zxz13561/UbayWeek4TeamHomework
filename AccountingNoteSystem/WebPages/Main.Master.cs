﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebPages
{
    public partial class Main : System.Web.UI.MasterPage
    {
        public string pageTitle { get; set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}