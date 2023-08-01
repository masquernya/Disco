import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import UserListCard from "../userListCard";
import s from './index.module.css';
import {getUser} from "../../lib/globalState";
import Link from "next/link";
import Router from "next/router";
const stillNeedSetupHeader = `You still need to finish setting up your account before you'll be able to match with people.`;

export default function Users(props) {
  const [users, setUsers] = useState(null);
  const usersToExclude = useRef([]);
  const [mutualState, setMutualState] = useState(null);
  const [hasDiscord, setHasDiscord] = useState(null);
  const [discordUrl,setDiscordUrl] = useState(null);
  const refreshUsers = () => {
    api.request('/api/user/FetchPotentialFriendsV1').then(data => {
      setUsers(data.body.data.filter(v => !usersToExclude.current.includes(v.accountId)));
    }).catch(e => {
      if (e.code === 'UnauthorizedException') {
        Router.push('/join');
      }else{
        throw e;
      }
    })
  }
  const onAcceptOrDecline = (v) => {
    usersToExclude.current.push(v.accountId);
    let newUsers = users.filter(x => x.accountId !== v.accountId);
    if (newUsers.length === 0) {
      setUsers(null);
      refreshUsers();
    }else{
      setUsers(newUsers);
    }
  }
  useEffect(() => {
    refreshUsers();
    api.request('/api/user/Discord').then(d => {
      setHasDiscord(!!d.body);
      if (!d.body) {
        api.request('/api/user/DiscordLinkUrl', {
          method: 'POST',
        }).then(data => {
          setDiscordUrl(data.body.redirectUrl);
        }).catch(e => {
          // ignore
        })
      }
    }).catch(e => {
      // ignore
    })
  }, []);

  const hasTags = !getUser() ? null : (getUser().data && getUser().data.tags.length > 0);

  return <div className='container min-vh-100'>
    {
      mutualState ? <div className={s.mutualModal}>
        <div className='container'>
          <div className='row'>
            <div className='col-12 col-lg-6 mx-auto'>
              <div className={s.mutualCard}>
                <h3 className={s.mutualTitle}>It's Mutual!</h3>
                <p>You and "{mutualState.displayName}" both liked each other. You can send a friend request to their discord account below:</p>
                {
                  mutualState.discord ? <p className={s.discord}>{mutualState.discord}</p> : <p>It looks like @{mutualState.username} removed their Discord account after liking you. You can check their account again later in the "Liked" tab to see if they add back their Discord account.</p>
                }
                <button className='btn btn-outline-danger mt-4' onClick={() => {
                  setMutualState(null);
                }}>Dismiss</button>
              </div>
            </div>
          </div>
        </div>
      </div> : null
    }
    <div className='row mt-4'>
      <div className='col-12'>
        {
          (hasTags === false) ? <div className={s.alert}>
            <span className={s.alertHeader}>{stillNeedSetupHeader}</span> Add at least one tag in <Link href={'/me'}>Settings</Link>.
          </div> : (hasDiscord === false && discordUrl) ? <div className={s.alert}>
            <span className={s.alertHeader}>{stillNeedSetupHeader}</span> Click <Link href={discordUrl}>here</Link> to attach your discord account.
          </div> : null
        }
        <h3 className='fw-bold text-uppercase'>Users</h3>
        {
          (users === null && hasDiscord && hasTags) ? <div className='justify-content-center d-flex'><div className='spinner-border' /></div> : null
        }
        {
          (users && users.length === 0 && hasDiscord && hasTags) ? <div>
            <p className='text-center mt-4'>No Users</p>
            <p className='text-center'>It looks like we have nobody matching your tags right now. Check back later!</p>
          </div> : null
        }
       <div className='row'>
           {
             users ? users.map(v => {
               return <div className='col-12 col-lg-6' key={v.accountId}>
                 <UserListCard user={v} accept={() => {
                   onAcceptOrDecline(v);
                   api.request('/api/user/UpdateRelationship', {
                     method: 'POST',
                     body: {
                       targetAccountId: v.accountId,
                       status: 'Accepted',
                     }
                   }).then(d => {
                     console.log('response',d.body);
                    if (d.body.isMutualLike) {
                      console.log('is mutual');
                      api.request('/api/user/'+v.accountId+'/Discord').then(disc => {
                        setMutualState({
                          ...v,
                          discord: disc.body.displayString,
                        });
                      })
                    }
                   })
                 }} decline={() => {
                   onAcceptOrDecline(v);
                   api.request('/api/user/UpdateRelationship', {
                     method: 'POST',
                     body: {
                       targetAccountId: v.accountId,
                       status: 'Ignored',
                     }
                   });
                 }} />
               </div>
             }) : null
           }
       </div>
      </div>
    </div>
  </div>
}