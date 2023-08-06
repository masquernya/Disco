import {useEffect, useState} from "react";
import api from "../../lib/api";
import modal from '../../styles/Modal.module.css';

export default function Tags() {
  const [unfilteredTags, setUnfilteredTags] = useState(null);
  const [filteredTags, setFilteredTags] = useState(null);
  const loadTags = () => {
    setUnfilteredTags(null);
    setFilteredTags(null);
    api.request('/api/user/FetchUnfilteredTopTags').then(d => {
      setUnfilteredTags(d.body);
    });
    api.request('/api/user/TopTags').then(d => {
      setFilteredTags(d.body);
    })
  }
  useEffect(() => {
    loadTags();
  }, []);

  const [approve, setApprove] = useState(null);
  const [tagDisplayName, setTagDisplayName] = useState('');
  const [tagBeingAdded, setTagBeingAdded] = useState(false);

  return <div className='container min-vh-100'>
    {
      approve ? <div className={modal.modal}>
        <div className={modal.content}>
          <h3 className='fw-bold mb-0'>Add "{approve.tag}"</h3>
          <p>Enter a display name for this tag.</p>
          <input placeholder={'Tag Display Name'} type='text' className='form-control' value={tagDisplayName} onChange={e => {
            setTagDisplayName(e.target.value);
          }} />
          <button className='btn btn-primary' disabled={!tagDisplayName || tagBeingAdded} onClick={() => {
            setTagBeingAdded(true);
            api.request('/api/user/ApproveTopTag', {
              method: 'POST',
              body: {
                tag: approve.tag,
                displayTag: tagDisplayName,
              },
            }).then(() => {
              // OK
              setUnfilteredTags(unfilteredTags.filter(v => v.tag !== approve.tag)); // ironically we filter the unfiltered tags...
              setTagBeingAdded(false);
              setApprove(null);
              api.request('/api/user/TopTags').then(d => {
                setFilteredTags(d.body);
              })
            }).catch(err => {
              alert(err.message);
              setTagBeingAdded(false);
            })
          }}>Add</button>
          <button className='btn btn-outline-secondary' onClick={() => {
            setApprove(null);
            setTagDisplayName('');
          }}>Close</button>
        </div>
      </div> : null
    }
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold'>Top Tags</h3>
        <p>This is for approving tags that appear in the "top tags" list. Without it, people could do things like put websites URLs so they appear in top tags, put illegal or ToS violation-related stuff, or even just be gross (e.g. top 10 tags would all be porn related).</p>
        <div className='row'>
          <div className='col-12 col-lg-6'>
            <p className='fw-bold'>Unfiltered Tags Leaderboard</p>
            <table className='table'>
              <thead>
              <tr>
                <th>Tag</th>
                <th>Count</th>
                <th>Actions</th>
              </tr>
              </thead>
              <tbody>
              {
                unfilteredTags ? unfilteredTags.map(v => {
                  return <tr key={v.tag}>
                    <td>{v.tag}</td>
                    <td>{v.count}</td>
                    <td>
                      <button className='btn btn-primary' onClick={() => {
                        setApprove(v);
                        setTagDisplayName(v.tag.slice(0, 1).toUpperCase() + v.tag.slice(1));
                      }}>Approve</button>
                    </td>
                  </tr>
                }) : null
              }
              </tbody>
            </table>
          </div>
          <div className='col-12 col-lg-6'>
            <p className='fw-bold'>Top Approved Tags</p>
            <table className='table'>
              <thead>
              <tr>
                <th>DTag</th>
                <th>Tag</th>
                <th>Count</th>
                <th>Actions</th>
              </tr>
              </thead>
              <tbody>
              {
                filteredTags ? filteredTags.map(v => {
                  return <tr key={v.tag}>
                    <td>{v.displayTag}</td>
                    <td>{v.tag}</td>
                    <td>{v.count}</td>
                    <td>
                      <button className='btn btn-danger' onClick={() => {
                        api.request('/api/User/DeleteTopTag', {
                          method: 'POST',
                          body: {
                            tag: v.tag,
                          }
                        }).then(() => {
                          setFilteredTags(filteredTags.filter(x => x !== v));
                        })
                      }}>Remove</button>
                    </td>
                  </tr>
                }) : null
              }
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
}