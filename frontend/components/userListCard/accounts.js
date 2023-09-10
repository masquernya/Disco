import s from './userListCard.module.css';
function AccountEntry({name, display, url}) {
  return <div className={s.accountEntry}>
    <div className={s.accountName + ' text-uppercase'}>{name}</div>
    <div className={s.accountDisplay}>
      {
        url ? <a href={url} className={s.accountUrl} target='_blank' rel='noopener nofollow noreferrer'>{display}</a> : display
      }
    </div>
  </div>
}

export default function Accounts({socialMedia}) {
  if (!socialMedia || !socialMedia.length) {
    return <p className='mb-0'>User does not have any social accounts</p>
  }
  return <div className={s.accountsContainer}>
    {
      socialMedia.map(v => {
        const url = v.type === 'Matrix' && v.displayString ? `https://matrix.to/#/${encodeURI(v.displayString)}` : undefined;
        return <AccountEntry key={v.displayString + v.type} name={v.type} display={v.displayString || 'Hidden'} url={url} />
      })
    }
  </div>
}