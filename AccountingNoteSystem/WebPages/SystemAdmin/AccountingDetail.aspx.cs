﻿using DataBaseFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UserAuthentication;

namespace WebPages.SystemAdmin
{
    public partial class AccountingDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // write page title
            Admin mainMaster = this.Master as Admin;
            mainMaster.MyTitle = "流水帳紀錄系統 - 後台 : 紀錄編輯";

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

            // check is postback
            if (!this.IsPostBack)
            {
                // Check is create mode or edit mode
                if (this.Request.QueryString["ID"] == null)
                {
                    this.btnDelete.Visible = false;
                }
                else
                {
                    this.btnDelete.Visible = true;

                    string idText = this.Request.QueryString["ID"];
                    int id;
                    if (int.TryParse(idText, out id))
                    {
                        var drAccounting = AccountingManager.GetAccounting(id, currentUser.ID.ToString());
                        if (drAccounting == null)
                        {
                            this.ltlMsg.Text = "Data doesn't exist";
                            this.btnSave.Visible = false;
                            this.btnDelete.Visible = false;
                        }
                        else
                        {
                            // if(drAccounting["UserID"].ToString() == dr["ID"].ToString())

                            this.ddlActType.SelectedValue = drAccounting["ActType"].ToString();
                            this.txtAmount.Text = drAccounting["Amount"].ToString();
                            this.txtCaption.Text = drAccounting["Caption"].ToString();
                            this.txtDesc.Text = drAccounting["Body"].ToString();
                        }
                    }
                    else
                    {
                        this.ltlMsg.Text = "ID is required.";
                        this.btnSave.Visible = false;
                        this.btnDelete.Visible = false;
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            List<string> msgList = new List<string>();

            // check input have error
            if (!this.CheckInput(out msgList))
            {
                this.ltlMsg.Text = string.Join("<br/>", msgList);
                return;
            }

            // check editor user is same data user
            var currentUser = AuthManager.GetCurrecntUser();
            if (currentUser == null)
            {
                this.Session["UserLoginInfo"] = null;
                Response.Redirect("/Login.aspx");
                return;
            }

            // collect input data
            string userID = currentUser.ID.ToString();
            string actTypeText = this.ddlActType.SelectedValue;
            string amountText = this.txtAmount.Text;
            string caption = this.txtCaption.Text;
            string body = this.txtDesc.Text;

            int amount = Convert.ToInt32(amountText);
            int actType = Convert.ToInt32(actTypeText);

            // get data ID from query string
            string idText = this.Request.QueryString["ID"];

            // check data ID
            if (string.IsNullOrWhiteSpace(idText))
            {
                // Excute 'Insert into database'
                AccountingManager.CreateAccounting(userID, caption, amount, actType, body);
            }
            else
            {
                int id;
                if (int.TryParse(idText, out id))
                {
                    AccountingManager.UpdateAccounting(id, userID, caption, amount, actType, body);
                }
            }

            Response.Redirect("/SystemAdmin/AccountingList.aspx");
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // get data ID
            string idText = this.Request.QueryString["ID"];

            // Empty ID
            if (string.IsNullOrWhiteSpace(idText))
                return;

            // delete data by ID
            int id;
            if (int.TryParse(idText, out id))
            {
                AccountingManager.DeleteAccounting(id);
            }

            // redirect to list web
            Response.Redirect("/SystemAdmin/AccountingList.aspx");
        }

        /// <summary> 檢查輸入選項 </summary>
        /// <param name="errorMsgList"> 錯誤訊息 </param>
        /// <returns> Boolean value </returns>
        private bool CheckInput(out List<string> errorMsgList)
        {
            List<string> msgList = new List<string>();

            // Type
            if (this.ddlActType.SelectedValue != "0" && this.ddlActType.SelectedValue != "1")
            {
                msgList.Add("Type must be 0 or 1");
            }

            // Amount
            if (string.IsNullOrWhiteSpace(this.txtAmount.Text))
            {
                msgList.Add("Amount is required");
            }
            else
            {
                // trans text to int, check it is success 
                int tempInt;
                if (!int.TryParse(this.txtAmount.Text, out tempInt))
                {
                    msgList.Add("Amount must be a number.");
                }
                if (tempInt < 0 || tempInt > 1000000)
                {
                    msgList.Add("Amount must be between 0 ~ 10,000,000.");
                }
            }

            errorMsgList = msgList;

            // check have error
            if (msgList.Count == 0)
                return true;
            else
                return false;
        }
    }
}