// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zel.DataAccess.Entity;
using Zel.DataAccess.Entity.Auditing;

namespace Zel.DataAccess.Tests.TestHelpers.Zel
{
    [UniqueConstraint("UIX_Employee_SSN", "SSN")]
    [UniqueConstraint("UIX_Employee_FirstName-LastName", "FirstName"
         , "LastName")]
    [Table("Employee", Schema = "dbo")]
    public class Employee : IEntity, IAuditCreatedByName, IAuditCreatedOn, IAuditModifiedByName, IAuditModifiedOn
    {
        [Key]
        public long EmployeeId { get; set; }

        [Required]
        [DisplayName("First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(9)]
        public string SSN { get; set; }

        [ParentEntity(typeof(Employer), "The specified employer doesn't exist.")]
        public int? EmployerId { get; set; }

        #region IAuditCreatedByName Members

        public string CreatedBy { get; set; }

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
    }
}