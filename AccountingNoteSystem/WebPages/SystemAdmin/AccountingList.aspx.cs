﻿using DataBaseFunctions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UserAuthentication;

namespace WebPages.SystemAdmin
{
    public partial class AccountingList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // write page title
            Admin mainMaster = this.Master as Admin;
            mainMaster.MyTitle = "流水帳紀錄系統 - 後台 : 流水帳紀錄";

            // check login
            if (!AuthManager.IsLogined())
            {
                Response.Redirect("/Login.aspx");
                return;
            }

            var currentUser = AuthManager.GetCurrecntUser();

            // if user not exist, redirect to login page
            if (currentUser == null)
            {
                this.Session["UserLoginInfo"] = null;
                Response.Redirect("/Login.aspx");
                return;
            }

            // read accounting data
            var dt = AccountingManager.GetAccountingList(currentUser.ID.ToString());

            this.CountSubTotal(dt);

            // check data is empty 
            if (dt.Rows.Count > 0)
            {
                var dtPaged = this.GetPageDataTable(dt);

                this.plcNoData.Visible = false;
                this.gvAccountList.DataSource = dtPaged;
                this.gvAccountList.DataBind();

                // 顯示頁數功能
                this.ucPager.TotalSize = dt.Rows.Count;
                this.ucPager.Bind();
            }
            else
            {
                this.gvAccountList.Visible = false;
                this.plcNoData.Visible = true;
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Response.Redirect("/SystemAdmin/AccountingDetail.aspx");
        }

        protected void gvAccountList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // get grid view row
            var row = e.Row;

            // select act type row
            if (row.RowType == DataControlRowType.DataRow)
            {
                // set row label
                Label lbl = row.FindControl("lblActType") as Label;

                // get act type data
                var dr = e.Row.DataItem as DataRowView;
                int actType = dr.Row.Field<int>("ActType");

                if (actType == 0)
                {
                    lbl.Text = "支出";
                    lbl.ForeColor = Color.Red;
                }
                else
                {
                    lbl.Text = "收入";
                    lbl.ForeColor = Color.Green;
                }
            }
        }

        #region Methods
        /// <summary> 獲得目前頁數 </summary>
        /// <returns> int 頁數 </returns>
        private int GetCurrentPage()
        {
            // get what page
            string pageText = Request.QueryString["Page"];
            if (string.IsNullOrWhiteSpace(pageText))
                return 1;

            int intPage;
            if (!int.TryParse(pageText, out intPage))
                return 1;

            if (intPage <= 0)
                return 1;

            return intPage;
        }

        /// <summary> 取得頁面數量的資料 </summary>
        /// <param name="dt"></param>
        /// <returns> DataTable 資料 </returns>
        private DataTable GetPageDataTable(DataTable dt)
        {
            DataTable dtPaged = dt.Clone();
            // dt.Copy() will error when no data inside data table

            int startIndex = (this.GetCurrentPage() - 1) * 10;
            int endIndex = this.GetCurrentPage() * 10;

            if (endIndex > dt.Rows.Count)
                endIndex = dt.Rows.Count;

            for (var i = startIndex; i < endIndex; i++)
            {
                // create new data row
                DataRow dr = dt.Rows[i];
                var drNew = dtPaged.NewRow();

                foreach (DataColumn dc in dt.Columns)
                {
                    // get value
                    drNew[dc.ColumnName] = dr[dc];
                }

                // Add row data
                dtPaged.Rows.Add(drNew);
            }

            return dtPaged;
        }

        /// <summary> 計算流水帳小計 </summary>
        /// <param name="dt">User Data Table</param>
        private void CountSubTotal(DataTable dt)
        {
            // count subtotal
            int _income = 0;
            int _spand = 0;
            foreach (DataRow subDr in dt.Rows)
            {
                if ((int)subDr["ActType"] == 0)
                    _spand += Convert.ToInt32(subDr["Amount"]);
                if ((int)subDr["ActType"] == 1)
                    _income += Convert.ToInt32(subDr["Amount"]);
            }
            this.ltlSubtotal.Text = $"小計 : {_income - _spand}";
        }
        #endregion
    }
}