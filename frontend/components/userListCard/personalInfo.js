import s from './userListCard.module.css';
import { FaCalendar } from "@react-icons/all-files/fa/FaCalendar";
import { FaGenderless } from '@react-icons/all-files/fa/FaGenderless';


export default function PersonalInfo({age, gender, pronouns}) {
  return <div className={s.personalContainer}>
    {age ? <div>
      <FaCalendar /> <span className={s.age}>{age} Years Old</span>
    </div> : null}

    {gender ? <span className='mt-2'>
      <span>{gender}</span>
    </span> : null}
    {gender && pronouns ? <FaGenderless className={s.genderPronounSep} /> : null}
    {pronouns ? pronouns : null}
  </div>
}