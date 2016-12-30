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
    [Table("Log", Schema = "dbo")]
    [DisplayName("Log")]
    public class Log : IEntity, IAuditCreatedOn, IAuditCreatedByName
    {
        [Key]
        public int LogId { get; private set; }

        [Required]
        [MaxLength(32)]
        public string GroupIdentifier { get; set; }

        [Required]
        [MaxLength(15)]
        public string Type { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }

        [MaxLength(500)]
        public string UrlReferrer { get; set; }

        [Required]
        public string Message { get; set; }

        [MaxLength(50)]
        public string MachineName { get; set; }

        [MaxLength(255)]
        public string ApplicationPath { get; set; }

        public string Data { get; set; }

        [MaxLength(50)]
        public string Source { get; set; }

        [MaxLength(32)]
        public string SourceId { get; set; }

        [MaxLength(5)]
        public string Code { get; set; }

        public bool Handled { get; set; }

        #region IAuditCreatedByName Members

        public string CreatedBy { get; set; }

        #endregion

        #region IAuditCreatedOn Members

        public DateTime CreatedOn { get; set; }

        #endregion
    }
}