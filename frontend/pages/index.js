import Head from 'next/head';
import HomeComponent from '../components/home';

export default function Home() {
  return <>
    <Head>
      <title>Make Discord Friends with DiscoFriends</title>
      <meta name='description' content='Join the DiscoFriends community and make tons of friends on Discord!' />
    </Head>
    <HomeComponent />
  </>
}