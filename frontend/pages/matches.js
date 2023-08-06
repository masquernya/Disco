import {useEffect, useState} from "react";
import api from "../lib/api";
import UserListCard from "../components/userListCard";
import Router from "next/router";
import Head from "next/head";

export default function Matches() {
  const [matches, setMatches] = useState(null);
  useEffect(() => {
    (async () => {
      let startId = 0;
      let allUsers = [];
      while (true) {
        let entries = await api.request('/api/user/FetchMatches?startId=' + startId);
        if (entries.body.data.length > 0) {
          startId = entries.body.data[entries.body.data.length - 1].accountId;
        }
        for (const item of entries.body.data) {
          allUsers.push(item);
        }
        if (entries.body.data.length < 100)
          break;
        break;
      }

      setMatches(allUsers);
    })().catch(e => {
      if (e.code === 'UnauthorizedException') {
        Router.push('/join');
      }else{
        throw e;
      }
    })
  }, []);

  return <div className='container min-vh-100'>
    <Head>
      <title>Matches - DiscoFriends</title>
    </Head>
    <div className='row mt-4 mb-4'>
      <div className='col-12'>
        <h3 className='text-uppercase fw-bold'>Matches</h3>
        <p className=''>People you liked who liked you back.</p>
      </div>
      {
        matches ? matches.map(v => {
          return <div className='col-12 col-lg-6' key={v.accountId}>
            <UserListCard user={v} hideButtons={true} />
          </div>
        }) : null
      }
    </div>
  </div>
}