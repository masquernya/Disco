import s from "./me.module.css";
import {getUser, setUser} from "../../lib/globalState";
import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";

export default function Tags(props) {
  const [topTags, setTopTags] = useState(null);
  const user = getUser();
  const [tags, setTags] = [
    user.data.tags,
    (newTags) => {
    setUser({
      ...user,
      data: {
        ...user.data,
        tags: newTags,
      }
    })
    },
  ]
  // const [tags, setTags] = useState(user.data.tags);
  const [locked, setLocked] = useState(false);
  const ref = useRef(null);

  const tryAddTag = (value) => {
    value = value.replaceAll(',', '');
    setLocked(true);
    api.request('/api/user/Tag', {
      method: 'PUT',
      body: {
        tag: value,
      },
    }).then((v) => {
      if (!tags.find(a => a.tagId === v.body.accountTagId)) {
        setTags([...tags, {
          tagId: v.body.accountTagId,
          tag: v.body.displayTag,
        }]);
      }
    }).catch(e => {

    }).finally(() => {
      setLocked(false);
    })
  }

  useEffect(() => {
    if (!topTags) {
      api.request('/api/User/TopTags').then(data => {
        setTopTags(data.body);
      })
    }
  }, [topTags]);

  return <div className={s.meSection}>
    <h4 className={s.subtitle + ' text-uppercase'}>Tags</h4>
    <p className={s.subtitleHelp}>Click on a tag to delete it.</p>
    <p className='fst-italic small'>Popular Tags: {
      topTags ? topTags.map(v => {
        return <span key={v.tag}>{v.displayTag} ({v.count}), </span>
      }) : <span className='spinner-border text-dark spinner-border-sm' />
    }</p>
    <div className={s.tagContainer}>
      {
        tags.map(v => {
          return <div key={v.tagId} className={s.tag} onClick={e => {
            if (locked) return;
            setLocked(true);
            api.request('/api/user/Tag', {
              method: 'DELETE',
              body: {
                tagId: v.tagId,
              },
            }).then(() => {
              //
              setTags(tags.filter(t => t.tagId !== v.tagId));
            }).catch(e => {

            }).finally(() => {
              setLocked(false);
            })
          }}>
            {v.tag}
          </div>
        })
      }
      <input disabled={locked} className={s.description} ref={ref} placeholder='Add a tag' onKeyUp={e => {
        if (e.key === 'Enter' || e.key === ',') {
          tryAddTag(ref.current.value);
          ref.current.value = '';
        }
      }}/>
      <div>
        <button className={s.saveButton} onClick={() => {
          tryAddTag(ref.current.value);
          ref.current.value = '';
        }}>Add Tag</button>
      </div>
    </div>
  </div>
}