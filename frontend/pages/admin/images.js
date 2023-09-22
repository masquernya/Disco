import {useEffect, useState} from "react";
import api from "../../lib/api";
import Link from "next/link";

export default function Images() {
  const [images, setImages] = useState(null);
  const loadImages = () => {
    setImages(null);
    api.request('/api/user/FetchAllImagesAwaitingReview').then(d => {
      setImages(d.body);
    })
  }
  useEffect(() => {
    loadImages();
  }, []);

  const toggleImage = (image, status) => {
    setImages(images => images.filter(v => v.image.userUploadedImageId !== image.userUploadedImageId));
    return api.request('/api/user/' + status, {
      method: 'POST',
      body: {
        imageId: image.userUploadedImageId
      },
    }).then(() => {
      // OK
    }).catch(err => {
      alert(err.message);
    })
  }

  return <div className='container min-vh-100'>
    <div className='row mt-4'>
      <div className='col-12'>
        <h3 className='fw-bold'>Unreviewed Images</h3>
        <div className='row'>
          {
            !images ? <div className='col-12'>
              <div className='spinner-border spinner-sm mx-auto d-block text-dark' />
            </div> : images.length === 0 ? <div className='col-12'>
              <p className='text-center'>Nothing to review.</p>
              <div className='d-flex justify-content-center'>
                <button className='btn btn-outline-info' onClick={() => {
                  loadImages();
                }}>Refresh</button>
              </div>
            </div> : images.map(image => {
              return <div className='col-12 col-sm-6 col-md-4 col-lg-3 mb-4' key={image.image.userUploadedImageId}>
                <div className='card bg-secondary h-100'>
                  <img src={image.image.url} alt='Picture' />
                  <div className='card-body'>
                    <button className='btn btn-success w-100' onClick={() => {
                      toggleImage(image.image, 'ApproveImage');
                    }}>Approve</button>
                    <button className='btn btn-danger w-100 mt-2' onClick={() => {
                      toggleImage(image.image, 'RejectImage');
                    }}>Reject</button>
                    <button className='btn btn-danger w-100 mt-2' onClick={() => {
                      let promises = [];
                      for (const account of image.accounts) {
                        promises.push(api.request('/api/user/BanUser', {
                          method: 'POST',
                          body: {
                            accountId: account.accountId
                          },
                        }));
                      }
                      for (const space of image.spaces) {
                        promises.push(api.request('/api/matrixspace/Ban?matrixSpaceId='+space.matrixSpaceId , {
                          method: 'POST',
                        }));
                      }
                      Promise.all(promises).then(() => {
                        toggleImage(image.image, 'RejectImage');
                      }).catch(err => {
                        alert(err.message);
                      })
                    }}>Reject + Ban</button>


                    <p className='mt-2'>Uploaded by {image.image.accountId === 1 ? 'Matrix Bot' : '#'+image.image.accountId}</p>
                    <p className='mb-0'>Accounts: {
                      image.accounts.length === 0 ? 'None' : image.accounts.map(v => {
                        return <span className='fw-bold' key={v.accountId}>{v.username} </span>
                      })
                    }</p>
                    <p className='mb-0'>Spaces: {
                      image.spaces.length === 0 ? 'None' : image.spaces.map(v => {
                        return <span className='fw-bold' key={v.matrixSpaceId}><Link href={`/spaces?id=${v.matrixSpaceId}`}>{v.name}</Link> </span>
                      })
                    }</p>
                  </div>
                </div>
              </div>
            })
          }
        </div>
        {
          images && images.length >= 2 ? <div className='row'>
            <div className='col-12'>
              <button className='btn btn-success mt-4' onClick={() => {
                if (prompt('Type "yes" to approve all ' + images.length + ' images on this page.') !== 'yes') {
                  return;
                }
                (async () => {
                  for (const image of images) {
                    await toggleImage(image.image, 'ApproveImage');
                  }
                })();
              }}>Approve Everything</button>
            </div>
          </div> : null
        }
      </div>
    </div>
  </div>
}