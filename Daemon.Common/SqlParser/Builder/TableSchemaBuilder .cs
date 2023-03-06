using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Service.Contract;
using Daemon.Common.Middleware;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Model;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.stmt;
using Daemon.Common.Enums.Extension;

namespace Daemon.Common.SqlParser.Builder
{
    public class TableSchemaBuilder
    {
        private static IFieldInfoService _fieldInfoService;

        private TableSchemaBuilder()
        {
            if (_fieldInfoService == null)
            {
                _fieldInfoService = ServiceLocator.Resolve<IFieldInfoService>();
            }
        }

        public static TableSchema buildFromAuto(string content)
        {
            if (_fieldInfoService == null)
            {
                _fieldInfoService = ServiceLocator.Resolve<IFieldInfoService>();
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }

            string[] words = content.Split(new char[] { ',', '，' }, StringSplitOptions.None);
            if (!words.Any() || words.Length > 20)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }

            IQueryable<FieldInfo> fieldInfoList = _fieldInfoService.GetFieldInfos();
            var nameFieldInfoDic = fieldInfoList.Where(r => words.Contains(r.Name)).AsEnumerable().GroupBy(r => r.Name).ToDictionary(r => r.Key, r => r.ToList());

            var fieldNameFieldInfoDic = fieldInfoList.Where(r => words.Contains(r.FieldName)).AsEnumerable().GroupBy(r => r.FieldName).ToDictionary(r => r.Key, r => r.ToList());
            TableSchema tableSchema = new TableSchema();
            tableSchema.TableName = "my_table";
            tableSchema.TableComment = "自动生成的表";
            List<Field> fieldList = new List<Field>();

            words.ToList().ForEach(word =>
            {
                Field field;
                List<FieldInfo> infoList = null;
                if (nameFieldInfoDic.TryGetValue(word, out List<FieldInfo> list))
                {
                    infoList = list;
                }
                //= nameFieldInfoDic[word];
                if (infoList == null)
                {
                    if (fieldNameFieldInfoDic.TryGetValue(word, out List<FieldInfo> listInfo))
                    {
                        infoList = listInfo;
                    }
                }

                if (infoList != null)
                {
                    field = Newtonsoft.Json.JsonConvert.DeserializeObject<Field>(infoList[0]?.Content);
                }
                else
                {
                    field = getDefaultField(word);
                }

                fieldList.Add(field);
            });
            tableSchema.FieldList = fieldList;

            return tableSchema;
        }

        public static TableSchema BuildFromSql(string sql)
        {
            try
            {
                TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
                sqlparser.sqltext = sql;
                int ret = sqlparser.parse();
                TableSchema tableSchema = new TableSchema();
                if (ret == 0)
                {
                    for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
                    {
                        tableSchema = analyzeStmt(sqlparser.sqlstatements.get(i));
                    }
                }
                return tableSchema;
            }
            catch (Exception e)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "请确认 SQL 语句正确");
            }
        }

        private static TableSchema analyzeStmt(TCustomSqlStatement stmt)
        {
            TableSchema tableSchema = null;
            switch (stmt.sqlstatementtype)
            {
                case ESqlStatementType.sstselect:
                    break;
                case ESqlStatementType.sstupdate:
                    break;
                case ESqlStatementType.sstcreatetable:
                    tableSchema = analyzeCreateStmt((TCreateTableSqlStatement)stmt);
                    break;
                case ESqlStatementType.sstaltertable:
                    break;
                case ESqlStatementType.sstcreateview:
                    break;
                default:
                    break;
            }
            return tableSchema;
        }

        private static TableSchema analyzeCreateStmt(TCreateTableSqlStatement pStmt)
        {
            TableSchema tableSchema = new TableSchema();
            tableSchema.TableName = pStmt.TableName.ObjectString.Replace("`", "");
            tableSchema.DbName = pStmt.TableName.SchemaString;
            tableSchema.TableComment = pStmt.TableComment?.String;
            var mysqlOptionList = pStmt.MySQLTableOptionList;
            for (var i = 0; i < mysqlOptionList.Count; i++)
            {
                if (mysqlOptionList[i].tableOption == ETableOption.EO_COMMENT)
                {
                    tableSchema.TableComment = mysqlOptionList[i].OptionValue.Replace("'", "").Replace(";", "");
                }
            }
            List<Field> fieldList = new List<Field>();
            var sqlArray = pStmt.String.Split(')', StringSplitOptions.RemoveEmptyEntries);
            var commentArray = sqlArray[sqlArray.Length - 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (string.IsNullOrEmpty(tableSchema.TableComment))
            {
                tableSchema.TableComment = commentArray[commentArray.Length - 1].Replace("'", "").Replace(";", "");
            }
            for (var i = 0; i < pStmt.ColumnList.Count; i++)
            {
                var currentColumn = pStmt.ColumnList.getColumn(i);
                var token = currentColumn.Constraints?.startToken?.nodesStartFromThisToken;
                Field field = new Field();

                if (currentColumn.String.ToLower().IndexOf("primary key") > -1)
                {
                    field.PrimaryKey = true;
                    field.MockType = MockTypeEnum.INCREASE.GetBaseDescription().DescriptionCN;
                }
                else
                {
                    field.MockType = MockTypeEnum.NONE.GetBaseDescription().DescriptionCN;
                }

                for (var j = 0; j < token?.Count; j++)
                {
                    var columnInfo = token[j];
                    if (columnInfo.GetType() == typeof(gudusoft.gsqlparser.nodes.TConstraint))
                    {
                        var tConstraint = (gudusoft.gsqlparser.nodes.TConstraint)columnInfo;
                        if (tConstraint.Constraint_type == EConstraintType.default_value)
                        {
                            field.DefaultValue = tConstraint.DefaultExpression?.String.Replace("`", "").ToString();
                            field.NotNull = true;
                        }
                        else
                        {
                            field.NotNull = false;
                        }
                    }
                }


                field.AutoIncrement = Convert.ToBoolean(currentColumn.Increment?.Val);
                if (currentColumn.String.ToLower().IndexOf("auto_increment") > -1)
                {
                    field.AutoIncrement = true;
                }
                field.Comment = currentColumn.columnComment?.String.Replace("'", "");

                field.FieldName = currentColumn.ColumnName?.String.Replace("`", "");
                field.FieldType = currentColumn.Datatype?.String.Replace("`", "");
                fieldList.Add(field);
            }
            tableSchema.FieldList = fieldList;
            return tableSchema;
        }

        /**
     * 获取默认字段
     *
     * @param word
     * @return
     */
        private static Field getDefaultField(string word)
        {
            Field field = new Field();
            field.FieldName = word;
            field.FieldType = "text";
            field.DefaultValue = "";
            field.NotNull = false;
            field.Comment = word;
            field.PrimaryKey = false;
            field.AutoIncrement = false;
            field.MockType = "";
            field.MockParams = "";
            field.OnUpdate = "";
            return field;
        }
    }
}