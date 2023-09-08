import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import s from '../userListCard/userListCard.module.css';
import styles from "../userListCard/userListCard.module.css";
import PersonalInfo from "../userListCard/personalInfo";
import Tags from "../userListCard/tags";
import Accounts from "../userListCard/accounts";
import Buttons from "../userListCard/buttons";
import Link from "next/link";

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

export function Space({matrixSpaceId, name, description, invite, imageUrl, is18Plus, memberCount, isManageable, edit, tags}) {
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
        <h3 className={styles.displayName}>{name}</h3>
        <p className={styles.username}>{memberCount.toLocaleString()} Members</p>
        {
          editMode ? <div className='form-check'>
            <label>Is 18+?</label>
            <input className='form-check-input' type='checkbox' checked={edit18Plus} onChange={e => {
              setEdit18Plus(!edit18Plus);
            }} />
          </div> : <a className={styles.joinButton} href={`https://matrix.to/#/${encodeURI(invite)}`} target='_blank'>Join</a>
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
        isManageable ? <button className={styles.buttonAccept + " mt-4"} onClick={() => {
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
    </div>
  </div>
}

export default function MatrixSpaces() {
  const [spaces, setSpaces] = useState(null);
  const [manageableSpaces, setManageableSpaces] = useState(null);
  const [show18Plus, setShow18Plus] = useState(false);

  useEffect(() => {
    api.request('/api/matrixspace/AllSpaces').then(spaces => {
      setSpaces(spaces.body);
    });

    api.request('/api/matrixspace/ManagedSpaces').then(managed => {
      setManageableSpaces(managed.body);
    })
  }, []);

  console.log('spaces',spaces);
  return <div className='container min-vh-100'>
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold text-uppercase'>Spaces</h3>
        <p>Discover matrix spaces to find new friends and discuss various topics.</p>
        <div className='form-check'>
          <label htmlFor='show-18-plus'>Show 18+</label>
          <input id='show-18-plus' className=' form-check-input' type='checkbox' checked={show18Plus} onChange={e => {
            setShow18Plus(!show18Plus);
          }} />
        </div>
      </div>
    </div>

    <div className='row'>
      {
        spaces ? spaces.filter(x => {
          if (show18Plus) return true;
          return !x.space.is18Plus;
        }).map(spaceInfo => {
          const {space, tags} = spaceInfo;
          return <div key={space.invite} className='col-12 col-lg-6'>
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
            }} />
          </div>
        }) : null
      }
    </div>
  </div>
}