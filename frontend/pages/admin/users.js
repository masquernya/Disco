import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import UserListCard from "../../components/userListCard";

export default function Reports() {
  const [users, setUsers] = useState(null);
  const [account, setAccount] = useState(null);
  const reasonRef = useRef(null);
  useEffect(() => {
    api.request('/api/user/FetchAllUsers').then(d => {
      setUsers(d.body);
    })
  }, []);

  return <div className='container min-vh-100'>
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold'>All Users</h3>
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
                  // setAccount(null);
                })
              }}>Ban</button>
            </div>
          </div> : null
        }
        <table className='table'>
          <thead>
          <tr>
            <th>#</th>
            <th>UName</th>
            <th>DName</th>
            <th>Pronouns/Gender/Age</th>
            <th>Created</th>
          </tr>
          </thead>
          <tbody>
          {
            users? users.map(v => {
              return <tr key={v.accountId}>
                <td>
                  <a href='#' onClick={e=> {
                    e.preventDefault();
                    api.request('/api/user/FullUserInfo?accountId=' + v.accountId).then(d => {
                      setAccount(d.body);
                    })
                  }}>{v.accountId}</a>
                </td>
                <td>{v.username}</td>
                <td>{v.displayName}</td>
                <td>{(v.pronouns || '-')}|{(v.gender || '-')}|{v.age || '-'}</td>
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