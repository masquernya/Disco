import s from './userListCard.module.css';

export default function Tags({tags}) {
  if (!tags || tags.length === 0) {
    return <p className='mb-0'>No tags to show</p>
  }
  return <div className={s.tagContainer}>
    {
      tags.sort((a, b) => a.displayTag.localeCompare(b.displayTag)).map(v => {
        return <div key={v.displayTag} className={s.tag}>{v.displayTag}</div>
      })
    }
  </div>
}