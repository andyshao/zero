// **************************************
// ���ɣ�CodeBuilder (http://www.fireasy.cn/codebuilder)
// ��Ŀ��Fireasy Zero
// ��Ȩ��Copyright Fireasy
// ���ߣ�Huangxd
// ʱ�䣺10/12/2017 21:26:08
// **************************************
using System;
using Fireasy.Data.Entity;
using System.ComponentModel.DataAnnotations;
using Fireasy.Data.Entity.Validation;

namespace Fireasy.Zero.Models
{
    /// <summary>
    /// ���� ʵ���ࡣ
    /// </summary>
    [Serializable]
    [EntityMapping("SysOrg", Description = "����")]
    [MetadataType(typeof(SysOrgMetadata))]
    public partial class SysOrg : LightEntity<SysOrg>
    {
        /// <summary>
        /// ��ȡ�����û���ID��
        /// </summary>

        [PropertyMapping(ColumnName = "OrgID", Description = "����ID", GenerateType = IdentityGenerateType.AutoIncrement, IsPrimaryKey = true, IsNullable = false)]
        [Key]
        public virtual int OrgID { get; set; }

        /// <summary>
        /// ��ȡ�����ñ��롣
        /// </summary>

        [PropertyMapping(ColumnName = "Code", Description = "����", Length = 50, IsNullable = true)]
        public virtual string Code { get; set; }

        /// <summary>
        /// ��ȡ���������ơ�
        /// </summary>

        [PropertyMapping(ColumnName = "Name", Description = "����", Length = 100, IsNullable = true)]
        public virtual string Name { get; set; }

        /// <summary>
        /// ��ȡ������ȫ�ơ�
        /// </summary>

        [PropertyMapping(ColumnName = "FullName", Description = "ȫ��", Length = 500, IsNullable = true)]
        public virtual string FullName { get; set; }

        /// <summary>
        /// ��ȡ������״̬��
        /// </summary>

        [PropertyMapping(ColumnName = "State", Description = "״̬", DefaultValue = StateFlags.Enabled, IsNullable = false)]
        public virtual StateFlags State { get; set; }

        /// <summary>
        /// ��ȡ����������
        /// </summary>

        [PropertyMapping(ColumnName = "OrderNo", Description = "����", IsNullable = true)]
        public virtual int OrderNo { get; set; }

        /// <summary>
        /// ��ȡ������ƴ���롣
        /// </summary>

        [PropertyMapping(ColumnName = "PyCode", Description = "ƴ����", Length = 100, IsNullable = true)]
        public virtual string PyCode { get; set; }

        /// <summary>
        /// ��ȡ����������(0:����,1:����)��
        /// </summary>

        [PropertyMapping(ColumnName = "Attribute", Description = "����(0:����,1:����)", IsNullable = false)]
        public virtual OrgAttribute Attribute { get; set; }

    }
	
    public class SysOrgMetadata
    {
        /// <summary>
        /// ���� OrgID ����֤���ԡ�
        /// </summary>
        [Required]
        public object OrgID { get; set; }

        /// <summary>
        /// ���� Code ����֤���ԡ�
        /// </summary>
        [StringLength(50)]
        public object Code { get; set; }

        /// <summary>
        /// ���� Name ����֤���ԡ�
        /// </summary>
        [StringLength(100)]
        [Required]
        public object Name { get; set; }

        /// <summary>
        /// ���� FullName ����֤���ԡ�
        /// </summary>
        [StringLength(500)]
        public object FullName { get; set; }

        /// <summary>
        /// ���� State ����֤���ԡ�
        /// </summary>
        [Required]
        public object State { get; set; }

        /// <summary>
        /// ���� OrderNo ����֤���ԡ�
        /// </summary>
        public object OrderNo { get; set; }

        /// <summary>
        /// ���� PyCode ����֤���ԡ�
        /// </summary>
        [StringLength(100)]
        public object PyCode { get; set; }

        /// <summary>
        /// ���� Attribute ����֤���ԡ�
        /// </summary>
        [Required]
        public object Attribute { get; set; }

    }
}

