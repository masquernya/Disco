import {useState} from "react";
import Head from "next/head";
import MatrixSpaces from "../components/matrixSpaces";
import api from "../lib/api";
import {useRouter} from "next/router";

export default function Spaces(props) {
  const router = useRouter();

  return <>
    <Head>
      <title>Matrix Spaces - DiscoFriends</title>
    </Head>
    <MatrixSpaces spaces={props.spaces} spaceId={router.query.id} />
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