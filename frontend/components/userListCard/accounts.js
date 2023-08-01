import s from './userListCard.module.css';
function AccountEntry({name, display}) {
  return <div className={s.accountEntry}>
    <div className={s.accountName + ' text-uppercase'}>{name}</div>
    <div className={s.accountDisplay}>{display}</div>
  </div>
}

export default function Accounts({socialMedia}) {
  if (!socialMedia || !socialMedia.length) {
    return <p className='mb-0'>User does not have any social accounts</p>
  }
  return <div className={s.accountsContainer}>
    {
      socialMedia.map(v => {
        return <AccountEntry key={v.displayString + v.type} name={v.type} display={v.displayString || 'Hidden'} />
      })
    }
  </div>
}