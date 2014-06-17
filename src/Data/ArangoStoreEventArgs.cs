/*
 * Created by SharpDevelop.
 * User: JuanJ
 * Date: 6/15/2014
 * Time: 8:08 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Commons.ArangoDb
{
	public class ArangoStoreEventArgs : EventArgs
	{
		public ArangoBaseModel Item { get; set; }
	}
}
