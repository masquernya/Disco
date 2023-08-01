import {useState} from "react";
import api from "../../lib/api";
import router from 'next/router';

const reportSections = [
  {
    id: 'Username',
    name: 'Their Username (The part after the "@" symbol)',
  },
  {
    id: 'DisplayName',
    name: 'Their Display Name',
  },
  {
    id: 'Avatar',
    name: 'Their Avatar',
  },
  {
    id: 'Description',
    name: 'Their Description',
  },
  {
    id: 'Tags',
    name: 'One or more of their tags',
  },
  {
    id: 'Pronouns',
    name: 'Their Pronouns',
  },
  {
    id: 'Gender',
    name: 'Their Gender',
  },
  {
    id: 'Other',
    name: 'Something Else',
  },
]

const reportReasons = [
  {
    id: 'Slur',
    name: 'Contains a slur',
  },
  {
    id: 'Harassment',
    name: 'Harassment',
  },
  {
    id: 'Url',
    name: 'Contains a harmful or sketchy URL',
  },
  {
    id: 'Spam',
    name: 'Spam',
  },
  {
    id: 'Illegal',
    name: 'Promotes or mentions an illegal activity'
  },
  {
    id: 'NotSafeForWorkUnder18',
    name: 'Promotes or mentions NSFW activity with account age below 18'
  },
  {
    id: 'NotSafeForWorkOutsideCorrectTags',
    name: 'Promotes or mentions NSFW activity outside the correct tags',
  },
  {
    id: 'Scam',
    name: 'Scam',
  },
]

export default function ReportForm({accountId}) {
  const [section, setSection] = useState('');
  const [reason, setReason] = useState('');
  const [locked, setLocked] = useState(false);

  const showJokeWarning = section === 'Pronouns' || section === 'Gender';

  return <div className='container min-vh-100 mt-4'>
    <div className='row'>
      <div className='col-12 col-lg-6 mx-auto'>
        <h3 className='fw-bold'>Report Account</h3>
        <p>Tell us how you think this user is breaking our Rules or Terms of Service.</p>
        {
          showJokeWarning ? <p className='mb-4'>Please note that joke pronouns and joke genders do not violate our Terms of Service.</p> : null
        }

        <p className='mb-0 fw-bold'>What part of their profile are you reporting?</p>
        <select className='form-control' value={section} onChange={e => {
          setSection(e.currentTarget.value);
        }}>
          <option value='' disabled={true}>Select an option</option>
          {reportSections.map(v => {
            return <option value={v.id} key={v.id}>{v.name}</option>
          })}
        </select>
        <p className='mb-0 fw-bold mt-4'>What is the reason?</p>
        <select className='form-control' value={reason} onChange={e => {
          setReason(e.currentTarget.value);
        }}>
          <option value='' disabled={true}>Select an option</option>
          {reportReasons.map(v => {
            return <option value={v.id} key={v.id}>{v.name}</option>
          })}
        </select>
        <button className='btn btn-outline-danger mt-4' disabled={!reason || !section || locked} onClick={() => {
          setLocked(true);
          api.request('/api/user/Report', {
            method: 'POST',
            body: {
              accountId: accountId,
              reason: reason,
              field: section,
            }
          }).then(() => {
            router.push('/list');
          }).catch(e => {
            alert(e.message);
          }).finally(() => {
            setLocked(false);
          })
        }}>Submit Report</button>
      </div>
    </div>
  </div>
}