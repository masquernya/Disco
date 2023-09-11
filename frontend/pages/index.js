import Head from 'next/head';
import HomeComponent from '../components/home';
import api from "../lib/api";

export default function Home(props) {
  return <>
    <Head>
      <title>Make Friends with DiscoFriends</title>
      <meta name='description' content='Join the DiscoFriends community and make tons of friends on Matrix!' />
    </Head>
    <HomeComponent spaces={props.spaces} />
  </>
}

export async function getStaticProps() {
  const spaces = await api.request('/api/matrixspace/AllSpaces');
  return {
    revalidate: 60,
    props: {
      spaces: spaces.body,
    }
  };
}