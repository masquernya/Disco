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
  const [hasMatrix, setHasMatrix] = useState(false);
  const [updateRelationshipLoading, setUpdateRelationshipLoading] = useState(false);

  const [fetchingMatrix, setFetchingMatrix] = useState(true);
  const [fetchingDiscord, setFetchingDiscord] = useState(true);
  const fetchingAnything = fetchingDiscord || fetchingMatrix;
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
    api.request('/api/user/Matrix').then(d => {
      setHasMatrix(!!d.body);
      setFetchingMatrix(false);
    })
    api.request('/api/user/Discord').then(d => {
      setHasDiscord(!!d.body);
      setFetchingDiscord(false);
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

  const hasAnySocialMedia = mutualState && (mutualState.discord || mutualState.matrix);
  const localUserHasAnySocial = (hasDiscord||hasMatrix);

  return <div className='container min-vh-100'>
    {
      mutualState ? <div className={s.mutualModal}>
        <div className='container'>
          <div className='row'>
            <div className='col-12 col-lg-6 mx-auto'>
              <div className={s.mutualCard}>
                <h3 className={s.mutualTitle}>It's Mutual!</h3>
                <p>You and "{mutualState.displayName}" both liked each other. You can send a friend request to their social media accounts below:</p>
                {
                  !hasAnySocialMedia ? <p>It looks like @{mutualState.username} removed their social media accounts after liking you. You can check their account again later in the "Matches" tab to see if they add back any accounts.</p> : null
                }
                {
                  mutualState.discord ? <p className={s.discord}>Discord: {mutualState.discord}</p> : null
                }
                {
                  mutualState.matrix ? <p className={s.discord}>Matrix: {mutualState.matrix}</p> : null
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
          fetchingAnything ? null : <>
            {(!hasTags) ? <div className={s.alert}>
              <span className={s.alertHeader}>{stillNeedSetupHeader}</span> Add at least one tag in <Link
              href={'/me'}>Settings</Link>.
            </div> : (!hasDiscord && discordUrl && !hasMatrix) ? <div className={s.alert}>
              <span className={s.alertHeader}>{stillNeedSetupHeader}</span> Click <Link href={discordUrl}>here</Link> to
              attach your discord account, or enter your Matrix username in <Link href={'/me'}>Settings</Link>.
            </div> : null}
          </>
        }
        <h3 className='fw-bold text-uppercase'>Users</h3>
        <p>Discover new people you haven't interacted with.</p>
        {
          (users === null && localUserHasAnySocial && hasTags) ? <div className='justify-content-center d-flex'><div className='spinner-border' /></div> : null
        }
        {
          (users && users.length === 0 && localUserHasAnySocial) ? <div>
            <h5 className='text-center mt-4 fw-bold'>No Users</h5>
            <p className='text-center'>It looks like we have nobody matching your tags right now. Check back later or add tags in <Link href='/me'>settings</Link>!</p>
          </div> : null
        }
       <div className='row'>
           {
             users ? users.map(v => {
               return <div className='col-12 col-lg-6' key={v.accountId}>
                 <UserListCard user={v} accept={() => {
                   if (updateRelationshipLoading)
                     return;
                   setUpdateRelationshipLoading(true);
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
                      Promise.all([
                        api.request('/api/user/'+v.accountId+'/Discord'),
                        api.request('/api/user/'+v.accountId+'/Matrix'),
                      ]).then(data => {
                        const [disc, matrix] = data;
                        setMutualState({
                          ...v,
                          discord: disc.body ? disc.body.displayString : null,
                          matrix: matrix.body ? ('@' + matrix.body.name + ':' + matrix.body.domain) : null,
                        })
                      }).finally(() => {
                        setUpdateRelationshipLoading(false);
                      })
                    }else{
                      setUpdateRelationshipLoading(false);
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