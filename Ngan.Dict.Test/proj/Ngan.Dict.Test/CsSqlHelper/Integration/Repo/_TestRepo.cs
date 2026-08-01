namespace Ngan.Dict.Test.CsSql.Integration.Repo;

using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Backend.Word.Dao;
using Tsinswreng.CsSql;
public partial class TestRepo(
	DaoWord DaoWord
	,IRepo<PoWord, IdWord> RepoWord
	,ITblMgr TblMgr
	,ISqlCmdMkr SqlCmdMkr
){
	protected DaoWord DaoWord{get;set;} = DaoWord;
	protected IRepo<PoWord, IdWord> RepoWord{get;set;} = RepoWord;
	protected ITblMgr TblMgr{get;set;} = TblMgr;
	protected ISqlCmdMkr SqlCmdMkr{get;set;} = SqlCmdMkr;
}
