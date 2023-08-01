import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import UserListCard from "../../components/userListCard";

export default function Reports() {
  const [reports, setReports] = useState(null);
  const [account, setAccount] = useState(null);
  const reasonRef = useRef(null);
  useEffect(() => {
    api.request('/api/user/PendingReports').then(d => {
      setReports(d.body);
    })
  }, []);

  return <div className='container min-vh-100'>
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold'>Unreviewed Reports</h3>
        {
          account ? <div className='row'>
            <div className='col-12'>
              <button className='btn btn-outline-danger btn-sm' onClick={() => {
                setAccount(null);
              }}>Close</button>
            </div>
            <div className='col-6'>
              <h5 className='fw-bold'>Preview</h5>
              <UserListCard user={{
                accountId: account.account.accountId,
                username: account.account.username,
                displayName: account.account.displayName,
                gender: account.account.gender,
                age: account.account.age,
                pronouns: account.account.pronouns,
                avatar: account.avatar ? {imageUrl: account.avatar.url} : null,
                description: account.description && account.description.description,
                socialMedia: account.discord ? [
                  {
                    displayString: account.discord.displayString,
                    type: 'Discord',
                  }
                ] : [],
                tags: account.accountTags,
              }}  hideButtons={true}/>
            </div>
            <div className='col-6'>
              <h5 className='fw-bold'>Actions</h5>
              <input ref={reasonRef} type='text' className='form-control' placeholder='Ban Reason' />
              <button className='btn-outline-danger btn mt-4' onClick={() => {
                api.request('/api/user/BanAccount', {
                  method: 'POST',
                  body: {
                    accountId: account.accountId,
                    reason: reasonRef.current.value || null,
                  }
                }).then(() => {
                  let toDelete = reports.filter(v => v.reportedAccountId === account.account.accountId);
                  let proms = [];
                  for (const item of toDelete) {
                    proms.push(api.request('/api/user/ResolveReport?reportId=' + item.accountReportId, {
                      method: 'POST',
                    }))
                  }
                  Promise.all(proms).then(() => {
                    setReports(reports.filter(v => v.reportedAccountId !== account.account.accountId));
                    setAccount(null);
                  })
                })
              }}>Ban</button>
            </div>
          </div> : null
        }
        <table className='table'>
          <thead>
          <tr>
            <th>#</th>
            <th>Account</th>
            <th>Field</th>
            <th>Reason</th>
            <th>Submitted</th>
          </tr>
          </thead>
          <tbody>
          {
            reports? reports.map(v => {
              return <tr key={v.accountReportId}>
                <td>{v.accountReportId}</td>
                <td>
                  <a href='#' onClick={e=> {
                    e.preventDefault();
                    api.request('/api/user/FullUserInfo?accountId=' + v.reportedAccountId).then(d => {
                      setAccount(d.body);
                    })
                  }}>#{v.reportedAccountId}</a>
                </td>
                <td>{v.field}</td>
                <td>{v.reason}</td>
                <td>{v.createdAt}</td>
              </tr>
            }) : null
          }
          </tbody>
        </table>
      </div>
    </div>
  </div>
}