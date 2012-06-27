using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Net.Sockets;


///         
//
///
namespace SocketClient
{
	public delegate void displayMessage(string msg);
	
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox msgViewBox;
		private System.Windows.Forms.TextBox sendBox;
		private System.Windows.Forms.Button sendButton;
		private System.Windows.Forms.Button connectButton;
		private System.Windows.Forms.TextBox usernameBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox userlistBox;
		private System.ComponentModel.Container components = null;
		//Some required Variables
		private string userID, userName;
		//Flag to check if this is the first communication with the server
		bool firstTime=true;
		private TcpClient chatClient;
		private byte[] recByte = new byte[1024];
		private StringBuilder myBuilder;

		//Constructor
		public Form1()
		{
			InitializeComponent();
			myBuilder = new System.Text.StringBuilder();
		}

		//Method use to process incomming messages
		public void GetMsg(IAsyncResult ar)
		{
			int byteCount;
			try
			{
				//Get the number of Bytes received
				byteCount = (chatClient.GetStream()).EndRead(ar);
				//If bytes received is less than 1 it means
				//the server has disconnected
				if(byteCount <1)
				{
					//Close the socket
					Disconnect();
					MessageBox.Show("Disconnected!!");
					return;
				}
				//Send the Server message for parsing
				BuildText(recByte,0,byteCount);
				//Unless its the first time start Asynchronous Read
				//Again
				if(!firstTime)
				{
					AsyncCallback GetMsgCallback = new AsyncCallback(GetMsg);
					(chatClient.GetStream()).BeginRead(recByte,0,1024,GetMsgCallback,this);
				}
			}
			catch(Exception ed)
			{
				Disconnect();
				MessageBox.Show("Exception Occured :"+ed.ToString());
			}
		}

		//Method to Process Server Response
		public void BuildText(byte[] dataByte, int offset, int count)
		{
			//Loop till the number of bytes received
			for(int i=offset; i<(count); i++)
			{
				//If a New Line character is met then
				//skip the loop cycle
				if(dataByte[i]==10)
					continue;
				//Add the Byte to the StringBuilder in Char format
				myBuilder.Append(Convert.ToChar(dataByte[i]));
			}
			char[] spliters ={'@'};
			//Check if this is the first message received
			if(firstTime)
			{
				//Split the string received at the occurance of '@'
				string[] tempString = myBuilder.ToString().Split(spliters);
				//If the Server sent 'sorry' that means there was some error
				//so we just disconnect the client
				if(tempString[0]=="sorry")
				{
					object[] temp = {tempString[1]};
					this.Invoke(new displayMessage(DisplayText),temp);
					Disconnect();
				}
				else
				{
					//Store the Client Guid 
					this.userID = tempString[0];
					//Loop through array of UserNames
					for(int i=1;i<tempString.Length;i++)
					{
						object[] temp = {tempString[i]};
						//Invoke the AddUser method
						//Since we are working on another thread rather than the primary 
						//thread we have to use the Invoke method
						//to call the method that will update the listbox
						this.Invoke(new displayMessage(AddUser),temp);
					}
					//Reset the flag
					firstTime=false;
					//Start the listening process again 
					AsyncCallback GetMsgCallback = new AsyncCallback(GetMsg);
					(chatClient.GetStream()).BeginRead(recByte,0,1024,GetMsgCallback,this);
				}
				
			}
			else
			{
				//Generally all other messages get passed here
				//Check if the Message starts with the ClientID
				//In which case we come to know that its a Server Command
				if(myBuilder.ToString().IndexOf(this.userID)>=0)
				{
					string[] tempString = myBuilder.ToString().Split(spliters);
					//If its connected command then add the user to the ListBox
					if(tempString[1]=="Connected")
					{
						object[] temp = {tempString[2]};
						this.Invoke(new displayMessage(AddUser),temp);
					}
					else if(tempString[1]=="Disconnected")
					{
						//If its disconnected command then remove the 
						//username from the list box
						object[] temp = {tempString[2]};
						this.Invoke(new displayMessage(RemoveUser),temp);
					}
				}
				else
				{
					//For regular messages append a Line terminator
					myBuilder.Append("\r\n");
					object[] temp = {myBuilder.ToString()};
					//Invoke the DisplayText method
					this.Invoke(new displayMessage(DisplayText),temp);
				}
			}
			//Empty the StringBuilder
			myBuilder = new System.Text.StringBuilder();
		}

		//Method to remove the user from the ListBox
		private void RemoveUser(string user)
		{
			if(userlistBox.Items.Contains(user))
				userlistBox.Items.Remove(user);
			//Display the left message
			DisplayText(user+" left chat\r\n");
		}

		//Method to Add a user to the ListBox
		private void AddUser(string user)
		{
			if(!userlistBox.Items.Contains(user))
				userlistBox.Items.Add(user);
			//If not for first time then display a connected message
			if(!firstTime)
				DisplayText(user+" joined chat\r\n");
		}
		
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					Disconnect();
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		
		//Method to send a message to the server
		public void SendText(string msg)
		{
			//Get a StreamWriter 
			System.IO.StreamWriter chatWriter = new System.IO.StreamWriter(chatClient.GetStream());
			chatWriter.WriteLine(msg);
			//Flush the stream
			chatWriter.Flush();
		}

		//Method to Display Text in the TextBox
		public void DisplayText(string msg)
		{
			msgViewBox.AppendText(msg);
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.connectButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.sendBox = new System.Windows.Forms.TextBox();
			this.msgViewBox = new System.Windows.Forms.TextBox();
			this.sendButton = new System.Windows.Forms.Button();
			this.usernameBox = new System.Windows.Forms.TextBox();
			this.userlistBox = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// connectButton
			// 
			this.connectButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.connectButton.Location = new System.Drawing.Point(656, 248);
			this.connectButton.Name = "connectButton";
			this.connectButton.TabIndex = 3;
			this.connectButton.Text = "Connect";
			this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
			// 
			// label1
			// 
			this.label1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(456, 248);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Username";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(0, 248);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "Message";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// sendBox
			// 
			this.sendBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.sendBox.Enabled = false;
			this.sendBox.Location = new System.Drawing.Point(64, 248);
			this.sendBox.Name = "sendBox";
			this.sendBox.Size = new System.Drawing.Size(296, 20);
			this.sendBox.TabIndex = 1;
			this.sendBox.Text = "";
			// 
			// msgViewBox
			// 
			this.msgViewBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left);
			this.msgViewBox.Multiline = true;
			this.msgViewBox.Name = "msgViewBox";
			this.msgViewBox.ReadOnly = true;
			this.msgViewBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.msgViewBox.Size = new System.Drawing.Size(528, 240);
			this.msgViewBox.TabIndex = 0;
			this.msgViewBox.Text = "";
			// 
			// sendButton
			// 
			this.sendButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.sendButton.Enabled = false;
			this.sendButton.Location = new System.Drawing.Point(368, 248);
			this.sendButton.Name = "sendButton";
			this.sendButton.TabIndex = 2;
			this.sendButton.Text = "Send";
			this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
			// 
			// usernameBox
			// 
			this.usernameBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.usernameBox.Location = new System.Drawing.Point(536, 248);
			this.usernameBox.Name = "usernameBox";
			this.usernameBox.Size = new System.Drawing.Size(112, 20);
			this.usernameBox.TabIndex = 5;
			this.usernameBox.Text = "";
			// 
			// userlistBox
			// 
			this.userlistBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.userlistBox.Location = new System.Drawing.Point(536, 0);
			this.userlistBox.Name = "userlistBox";
			this.userlistBox.Size = new System.Drawing.Size(200, 238);
			this.userlistBox.TabIndex = 4;
			// 
			// Form1
			// 
			this.AcceptButton = this.connectButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(736, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.label1,
																		  this.usernameBox,
																		  this.userlistBox,
																		  this.connectButton,
																		  this.sendButton,
																		  this.sendBox,
																		  this.msgViewBox});
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void sendButton_Click(object sender, System.EventArgs e)
		{
			if(sendBox.Text!="")
			{
				//Send Message
				SendText(sendBox.Text);
				sendBox.Text="";
			}
		}
		private void Disconnect()
		{
			if(chatClient!=null)
			{
				chatClient.Close();
				chatClient=null;
			}
			//Reset the Buttons and Variables
			userlistBox.Items.Clear();
			sendButton.Enabled=false;
			connectButton.Text="Connect";
			usernameBox.Enabled=true;
			sendBox.Enabled=false;
			this.AcceptButton=connectButton;
			firstTime=true;
			userID="";
		}

		private void connectButton_Click(object sender, System.EventArgs e)
		{
			//If user Cliked Connect
			if(connectButton.Text=="Connect"&&usernameBox.Text!="")
			{
				try
				{
					//Connect to server
					chatClient = new TcpClient("localhost",5151);
					DisplayText("Connecting to Server ...\r\n");
					//Start Reading
					AsyncCallback GetMsgCallback = new AsyncCallback(GetMsg);
					(chatClient.GetStream()).BeginRead(recByte,0,1024,GetMsgCallback,null);
					//Send the UserName
					SendText(usernameBox.Text);
					this.userName=usernameBox.Text;
					this.Text="Chat Client :"+userName;
					usernameBox.Text="";
					connectButton.Text="Disconnect";
					usernameBox.Enabled=false;
					sendButton.Enabled=true;
					sendBox.Enabled=true;
					this.AcceptButton=sendButton;
				}
				catch
				{
					Disconnect();
					MessageBox.Show("Can't connect to Server...");
				}
			}
			else if(connectButton.Text=="Disconnect")
			{
				Disconnect();
			}	
		}
	}
}
