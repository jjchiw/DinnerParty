﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DinnerParty.Models;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using Arango.Client;
using DinnerParty.Infrastructure;

namespace DinnerParty
{
    public class UserMapper : IUserMapper
    {
        private ArangoDatabase _db;

        public UserMapper(ArangoDatabase db)
        {
            this._db = db;

        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var whereOperation = new ArangoQueryOperation().Aql(_ =>
                                _.FILTER(_.Var("item.UserId"), ArangoOperator.Equal, _.Val(identifier.ToString())
                                ));

            var userRecord = _db.Query.IndexUserLogin(whereOperation).FirstOrDefault();

            return userRecord == null ? null : new UserIdentity() { UserName = userRecord.Username, FriendlyName = userRecord.FriendlyName };
        }

        public Guid? ValidateUser(string username, string password)
        {
            var whereOperation = new ArangoQueryOperation().Aql(_ =>
                               _.FILTER(_.Var("item.Username"), ArangoOperator.Equal, _.TO_STRING(_.Val(username))
                               .AND(_.Var("item.Password"), ArangoOperator.Equal, _.TO_STRING(_.Val(EncodePassword(password)))
                               )));

            var userRecord = _db.Query.IndexUserLogin(whereOperation).FirstOrDefault();

            if (userRecord == null)
            {
                return null;
            }

            return Guid.Parse(userRecord.UserId);
        }

        public Guid? ValidateRegisterNewUser(RegisterModel newUser)
        {
            var userRecord = new UserModel()
            {
                UserId = Guid.NewGuid(),
                LoginType = "DinnerParty",
                EMailAddress = newUser.Email,
                FriendlyName = newUser.Name,
                Username = newUser.UserName,
                Password = EncodePassword(newUser.Password)
            };

            var whereOperation = new ArangoQueryOperation().Aql(_ =>
                               _.FILTER(_.Var("item.EMailAddress"), ArangoOperator.Equal, _.TO_STRING(_.Val(userRecord.EMailAddress))
                               .AND(_.Var("item.LoginType"), ArangoOperator.Equal, _.TO_STRING(_.Val("DinnerParty")))
                               ));

            var existingUser = _db.Query.IndexUserLogin(whereOperation).FirstOrDefault();

            if (existingUser != null)
                return null;

            _db.Document.Create<UserModel>(ArangoModelBase.GetCollectionName<UserModel>(), userRecord);

            return userRecord.UserId;
        }

        private string EncodePassword(string originalPassword)
        {
            if (originalPassword == null)
                return String.Empty;

            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = ASCIIEncoding.Default.GetBytes(originalPassword);
            encodedBytes = md5.ComputeHash(originalBytes);

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes);
        }

    }
}