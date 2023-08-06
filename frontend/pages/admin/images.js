import {useEffect, useRef, useState} from "react";
import api from "../../lib/api";
import UserListCard from "../../components/userListCard";
import getConfig from "next/config";

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
    setImages(images.filter(v => v.userUploadedImageId !== image.userUploadedImageId));
    api.request('/api/user/' + status, {
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
              return <div className='col-12 col-sm-6 col-md-4 col-lg-3' key={image.userUploadedImageId}>
                <div className='card'>
                  <img src={image.url} alt='Profile picture' />
                  <div className='card-body'>
                    <p>Uploaded by #{image.accountId}</p>
                    <div className='row'>
                      <div className='col-6'>
                        <button className='btn btn-success w-100' onClick={() => {
                          toggleImage(image, 'ApproveImage');
                        }}>Approve</button>
                      </div>
                      <div className='col-6'>
                        <button className='btn btn-danger w-100' onClick={() => {
                          toggleImage(image, 'RejectImage');
                        }}>Reject</button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            })
          }
        </div>
      </div>
    </div>
  </div>
}