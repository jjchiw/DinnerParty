﻿/*
 * Created by SharpDevelop.
 * User: JuanJ
 * Date: 6/15/2014
 * Time: 6:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Arango.Client;

namespace Commons.ArangoDb
{
	public interface IArangoViewIndex
    {
        ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, string forItemName = "item");
    }
}
