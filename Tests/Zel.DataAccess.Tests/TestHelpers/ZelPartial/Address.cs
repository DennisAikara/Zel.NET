// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Entity.Auditing;

namespace Zel.DataAccess.Tests.TestHelpers.ZelPartial
{
    [Table("Address", Schema = "dbo")]
    public class Address : IUniqueIdentifier, IAuditCreatedBy, IAuditCreatedOn, IAuditModifiedByName, IAuditModifiedOn,
        IEntity
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        [DisplayName("Address 1")]
        [StringLength(50)]
        public string Address1 { get; set; }

        [StringLength(50)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        public string State { get; set; }

        [Required]
        [StringLength(15)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        #region IAuditCreatedBy Members

        public int CreatedBy { get; set; }

        #endregion

        #region IAuditCreatedOn Members

        public DateTime CreatedOn { get; set; }

        #endregion

        #region IAuditModifiedByName Members

        public string ModifiedBy { get; set; }

        #endregion

        #region IAuditModifiedOn Members

        public DateTime ModifiedOn { get; set; }

        #endregion

        #region IUniqueIdentifier Members

        public string UniqueIdentifier { get; private set; }

        #endregion
    }
}