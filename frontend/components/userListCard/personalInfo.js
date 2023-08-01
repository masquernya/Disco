import s from './userListCard.module.css';
export default function PersonalInfo({age, gender, pronouns}) {
  return <div className={s.personalContainer}>
    {age ? age : null}
    {age && (gender || pronouns) ? "|" : null}
    {gender ? gender : null}
    {gender && pronouns ? '|' : null}
    {pronouns ? pronouns : null}
  </div>
}