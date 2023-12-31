import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import s from '../userListCard/userListCard.module.css';
import styles from "../userListCard/userListCard.module.css";
import PersonalInfo from "../userListCard/personalInfo";
import Tags from "../userListCard/tags";
import Accounts from "../userListCard/accounts";
import Buttons from "../userListCard/buttons";
import Link from "next/link";
import config from "next/config";
import {getUser} from "../../lib/globalState";
import {useRouter} from "next/router";

function EditTags({matrixSpaceId, newTags, setNewTags}) {
  const [tag, setTag] = useState('');
  const [addingTag, setAddingTag] = useState(false);

  const addTag = () => {
    if (addingTag)
      return;
    setAddingTag(true);

    api.request(`/api/matrixspace/Tag`, {
      method: 'PUT',
      body: {
        matrixSpaceId,
        tag: tag,
      }
    }).then((data) => {
      setAddingTag(false);
      setTag('');
      if (newTags.find(x => x.matrixSpaceTagId === data.body.matrixSpaceTagId))
        return;

      setNewTags([... newTags, data.body]);
    }).catch(err => {
      alert(err.message);
      setAddingTag(false);
    })
  }
  return <div>
    <div className={styles.tagContainer}>
      {
        newTags.map(v => {
          return <div className={styles.tag} onClick={e => {
            if (addingTag)
              return;
            setAddingTag(true);
            api.request(`/api/matrixspace/Tag`, {
              method: 'DELETE',
              body: {
                matrixSpaceId,
                tagId: v.matrixSpaceTagId,
              }
            }).then(() => {
              setNewTags(newTags.filter(x => x.matrixSpaceTagId !== v.matrixSpaceTagId))
            }).finally(() => {
              setAddingTag(false);
            })
          }}>{v.displayTag}</div>
        })
      }
    </div>
    <div className={styles.addTagContainer}>
      <input placeholder='Add a Tag' type='text' className={styles.tagInput} value={tag} onChange={e => {
        setTag(e.currentTarget.value);
      }} onKeyDown={e => {
        if (e.key === 'Enter') {
          addTag();
        }
      }} />
      <button className={styles.addTagButton} onClick={() => {
        addTag();
      }}>Add</button>
    </div>
  </div>
}

export function Space({matrixSpaceId, name, description, invite, imageUrl, is18Plus, memberCount, isManageable, edit, tags, isAdmin}) {
  const [editMode, setEditMode] = useState(false);

  const [editDescription, setEditDescription] = useState(description);
  const [edit18Plus, setEdit18Plus] = useState(is18Plus);
  const [newTags, setNewTags] = useState(tags);

  const isDirty = (editDescription !== description) || (edit18Plus !== is18Plus) || (newTags !== tags);

  return <div className={styles.card + ' h-100'}>
    <div className={styles.cardPadding} />
    <div className={styles.cardHeader}>
      <div className={styles.cardHeaderItem}>
        <div className={styles.imageWrapper}>
          <img className={styles.image + ' sr-none'} src={imageUrl} />
        </div>
      </div>
      <div className={styles.cardHeaderItem + ' ms-4 w-100'}>
        <h3 className={styles.displayName}>
          <span>{name}</span>
        </h3>
        <p className={styles.username}>
          <span>{memberCount.toLocaleString()} Members</span>
        </p>
        {
          editMode ? <div className='form-check'>
            <label>Is 18+?</label>
            <input className='form-check-input' type='checkbox' checked={edit18Plus} onChange={e => {
              setEdit18Plus(!edit18Plus);
            }} />
          </div> : <a className={'btn btn-primary-alt text-center d-block w-100 ' + styles.joinButton} href={`https://matrix.to/#/${encodeURI(invite)}`} target='_blank'>Join</a>
        }
      </div>
    </div>
    <div className={styles.cardInner}>

      <p className={styles.heading + ' text-uppercase'}>Description</p>
      {
        editMode ? <textarea rows={4} className={styles.editDescription} value={editDescription || ''} onChange={e => {
          setEditDescription(e.currentTarget.value);
        }} />: <p className={styles.description}>{description  || '-'}</p>
      }

      <p className={styles.heading + ' text-uppercase'}>Tags</p>
      {
        editMode ? <EditTags newTags={newTags} setNewTags={setNewTags} matrixSpaceId={matrixSpaceId} /> : <Tags tags={tags.map(v => {
          return {
            displayTag: v.displayTag,
          }
        })} />
      }

      {
        isManageable ? <button className={"btn btn-primary mt-4"} onClick={() => {
          setEditMode(!editMode);
          if (editMode && isDirty) {
            let promises = [];
            if (edit18Plus !== is18Plus) {
              promises.push(api.request('/api/matrixspace/Set18Plus', {
                method: 'POST',
                body: {
                  matrixSpaceId: matrixSpaceId,
                  is18Plus: edit18Plus,
                }
              }));
            }

            Promise.all(promises).then(() => {
              //...
              edit({
                is18Plus: edit18Plus,
                description: editDescription,
              }, newTags);
            }).catch(err => {
              alert(err.message);
            })
          }
        }}>{editMode ? (isDirty ? 'Save Changes' : 'Close') : 'Edit'}</button> : null
      }
      {
        isAdmin ? <button className='btn btn-outline-danger mt-4' onClick={() => {
          if (prompt('Type "yes" to ban and delete this space.') !== 'yes') {
            return;
          }
          api.request('/api/matrixspace/Ban?matrixSpaceId=' + matrixSpaceId, {
            method: 'POST',
          }).then(() => {
            window.location.reload();
          }).catch(err => {
            alert(err.message);
          });
        }}>Delete</button> : null
      }
    </div>
  </div>
}

export default function MatrixSpaces(props) {
  const [spaces, setSpaces] = useState(props.spaces);
  const [manageableSpaces, setManageableSpaces] = useState(null);
  const [show18Plus, setShow18Plus] = useState(false);
  const [showSet18Plus, setShowSet18Plus] = useState(false);
  const [spaceId, setSpaceId] = useState(null);
  useEffect(() => {
    if (props.spaceId) {
      const id = parseInt(props.spaceId, 10);
      if (Number.isSafeInteger(id))
        setSpaceId(id);
    }
  }, [props.spaceId]);

  useEffect(() => {
    if (!props.spaces) {
      api.request('/api/matrixspace/AllSpaces').then(spaces => {
        setSpaces(spaces.body);
      });
    }

  }, [props.spaces]);

  useEffect(() => {
    const user = getUser();
    if (user) {
      if (user.isLoggedIn) {
        if (user.data && user.data.age >= 18) {
          setShowSet18Plus(true);
        }
        api.request('/api/matrixspace/ManagedSpaces').then(managed => {
          setManageableSpaces(managed.body);
        }).catch(err => {
          // probably not logged in. todo: alert?
        })
      }else{
        setShowSet18Plus(true);
      }
    }
  }, [getUser()]);
  const limit = props.limit || 100;
  const [offset, setOffset] = useState(0);
  const [query, setQuery] = useState('');

  const queryLower = query && query.toLowerCase();
  const matchingFilter = spaces ? spaces.filter(x => {
    if (show18Plus) return true;
    return !x.space.is18Plus;
  }).filter(x => {
    if (spaceId) {
      return x.space.matrixSpaceId === spaceId;
    }
    if (queryLower) {
      if (x.space.name.toLowerCase().includes(queryLower)) {
        x._searchPriority = 1;
        return true;
      }else if ((x.space.description || '').toLowerCase().includes(queryLower)) {
        x._searchPriority = 2;
        return true;
      }
      return false;
    }
    return true;
  }).sort((a,b) => {
    if (queryLower) {
      return a._searchPriority - b._searchPriority;
    }
    if (props.sort) {
      return props.sort(a, b);
    }
    return b.space.memberCount - a.space.memberCount;
  }) : null;

  const canGoPrevious = offset > 0;
  const canGoNext = matchingFilter && offset + limit < matchingFilter.length;

  const currentPage = Math.floor(offset / limit) + 1;
  const nextPage = currentPage + 1;

  return <div className='container min-vh-100'>
    {
      props.header !== false ? <div className='row mt-4'>
        <div className='col-12'>
          <h3 className='fw-bold text-uppercase'>Spaces</h3>
          <p>Discover matrix spaces to find new friends and discuss various topics. To add your space, invite our bot, {config().publicRuntimeConfig.matrixBotUsername}, to your space.</p>
          {showSet18Plus ? <div className='form-check'>
            <label htmlFor='show-18-plus'>Show 18+</label>
            <input id='show-18-plus' className=' form-check-input' type='checkbox' checked={show18Plus} onChange={e => {
              setShow18Plus(!show18Plus);
            }} />
          </div> : null}
        </div>

        <div className='col-12'>
          <p className='text-start mt-4 fw-bold'>{matchingFilter.length.toLocaleString()} Spaces Matching Filters</p>

          <input type='text' className='form-control mb-4' placeholder='Search' value={query} onChange={e => {
            setQuery(e.currentTarget.value);
          }} />

          <nav>
            <ul className="pagination">
              <li className="page-item">
                <a className={'page-link ' + (!canGoPrevious ? 'disabled' : '')} href="#" onClick={() => {
                  if (!canGoPrevious)
                    return;
                  setOffset(offset - limit);
                }}>Previous</a>
              </li>
              <li className="page-item"><a className="page-link disabled" href="#">{currentPage}</a></li>
              <li className="page-item">
                <a className={'page-link ' + (!canGoNext ? 'disabled' : '')} href="#" onClick={() => {
                  if (!canGoNext)
                    return;
                  setOffset(offset + limit);
                }}>Next</a>
              </li>
            </ul>
          </nav>
        </div>
      </div> : null
    }

    <div className='row'>
      {
        spaces && matchingFilter ? matchingFilter.slice(offset, offset + limit).map(spaceInfo => {
          const {space, tags} = spaceInfo;
          return <div key={space.invite} className='col-12 col-lg-6 mb-4'>
            <Space tags={tags} matrixSpaceId={space.matrixSpaceId} name={space.name} invite={space.invite} description={space.description} imageUrl={spaceInfo.imageUrl} is18Plus={space.is18Plus} memberCount={space.memberCount} isManageable={manageableSpaces && manageableSpaces.find(x => x.matrixSpaceId === space.matrixSpaceId)} edit={(newProps, newTags) => {
              for (let i = 0; i < spaces.length; i++) {
                const curr = spaces[i];
                if (curr.space.matrixSpaceId === space.matrixSpaceId) {
                  // apply edits
                  curr.space = {...curr.space, ...newProps}
                  curr.tags = newTags;
                }
              }
              setSpaces([...spaces]);
            }} isAdmin={getUser() && getUser().data && getUser().data.isAdmin} />
          </div>
        }) : null
      }
      {
        props.showMore ? <div className='col-12'>
          <div className='d-flex justify-content-center'>
            <Link href={'/spaces'} className={'btn btn-primary ps-4 pe-4 ' + s.showMore}>Show More</Link>
          </div>
        </div> : null
      }
    </div>
  </div>
}