package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.dao.DataAccessException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.UserRequest;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.service.UserService;

@RestController
public class UserController {

	@Autowired
	private UserService userService;

	@RequestMapping(value="/v1/users",method=RequestMethod.GET)
	public ResponseEntity<?> getUsers(
			@RequestParam(name="username",required=false) String username) {
		if (username != null) {
			UserInfo ui = userService.getUser(username);
			if (ui != null) {
				return new ResponseEntity<>(ui, HttpStatus.OK);
			} else {
				return new ResponseEntity<>(HttpStatus.NOT_FOUND);
			}
		}
		UserInfo[] uiArray = userService.getUsers();
		return new ResponseEntity<>(uiArray, HttpStatus.OK);
	}

	@RequestMapping(value="/v1/users/{uid:[0-9]+}",method=RequestMethod.GET)
	public ResponseEntity<?> getUserById(
			@PathVariable int uid) {
		UserInfo ui = userService.getUser(uid);
		if (ui != null) {
			return new ResponseEntity<>(ui, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/users",method=RequestMethod.POST)
	public ResponseEntity<?> createUser(
			@RequestBody UserRequest req) {
		try {
			UserInfo ui = userService.createUser(req.getUsername(), req.getPassword(), req.getRoles());
			return new ResponseEntity<>(ui, HttpStatus.CREATED);
		} catch (DataAccessException e) { // e.g. constraint violation
			return new ResponseEntity<>(e.getMessage(), HttpStatus.BAD_REQUEST);
		}
	}

	@RequestMapping(value="/v1/users/{uid:[0-9]+}",method=RequestMethod.PUT)
	public ResponseEntity<?> updateUser(
			@PathVariable int uid,
			@RequestBody UserRequest req) {
		UserInfo ui = userService.updateUser(uid, req.getUsername(), req.getPassword(), req.getRoles());
		if (ui != null) {
			return new ResponseEntity<>(ui, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/users/{uid:[0-9]+}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteUser(
			@PathVariable int uid) {
		UserInfo ui = userService.deleteUser(uid);
		if (ui != null) {
			return new ResponseEntity<>(ui, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
